using System;
using System.Linq;
using System.Threading.Tasks;
using EnglishLearningPlatformApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Authorization;
using Volo.Abp.Data;

namespace EnglishLearningPlatformApp.Content;

[Authorize(EnglishLearningPlatformAppPermissions.Content.Default)]
public class ContentAdminAppService : ApplicationService, IContentAdminAppService
{
    private readonly IRepository<ContentItem, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;
    private readonly IContentPermissionAuthorizer _permissionAuthorizer;

    public ContentAdminAppService(
        IRepository<ContentItem, Guid> repository,
        ICurrentTenant currentTenant,
        IClock clock,
        IContentPermissionAuthorizer permissionAuthorizer)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _clock = clock;
        _permissionAuthorizer = permissionAuthorizer;
    }

    public async Task<ContentItemDto> GetAsync(Guid id)
    {
        EnsureAuthenticated();
        await _permissionAuthorizer.CheckAsync(EnglishLearningPlatformAppPermissions.Content.Default);
        return Map(await GetItemAsync(id));
    }

    public async Task<PublishedContentVersionDto> GetPublishedAsync(Guid id)
    {
        EnsureAuthenticated();
        await _permissionAuthorizer.CheckAsync(EnglishLearningPlatformAppPermissions.Content.Default);
        var item = await GetItemAsync(id);
        var version = item.Versions
            .Where(x => x.Lifecycle == ContentLifecycleStatus.Published)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefault()
            ?? throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.ContentPublishedVersionMissing);
        return MapPublished(item, version);
    }

    [Authorize(EnglishLearningPlatformAppPermissions.Content.Create)]
    public async Task<ContentItemDto> CreateAsync(CreateContentInput input)
    {
        EnsureAuthenticated();
        await _permissionAuthorizer.CheckAsync(EnglishLearningPlatformAppPermissions.Content.Create);
        var tenantId = _currentTenant.Id
            ?? throw new AbpAuthorizationException("Content administration is available only inside a tenant.");
        var item = new ContentItem(GuidGenerator.Create(), tenantId, input.Type, GuidGenerator.Create(), input.Title);
        await _repository.InsertAsync(item, autoSave: true);
        return Map(item);
    }

    [Authorize(EnglishLearningPlatformAppPermissions.Content.Edit)]
    public async Task<ContentItemDto> UpdateDraftAsync(Guid id, UpdateContentDraftInput input)
    {
        EnsureAuthenticated();
        await _permissionAuthorizer.CheckAsync(EnglishLearningPlatformAppPermissions.Content.Edit);
        var item = await GetItemAsync(id);
        CheckConcurrency(item, input.ConcurrencyStamp);
        item.UpdateDraft(input.Title);
        await _repository.UpdateAsync(item, autoSave: true);
        return Map(item);
    }

    [Authorize(EnglishLearningPlatformAppPermissions.Content.Publish)]
    public async Task<ContentItemDto> PublishAsync(Guid id, ContentConcurrencyInput input)
    {
        EnsureAuthenticated();
        await _permissionAuthorizer.CheckAsync(EnglishLearningPlatformAppPermissions.Content.Publish);
        var item = await GetItemAsync(id);
        CheckConcurrency(item, input.ConcurrencyStamp);
        var publishedAt = _clock.Now;
        item.PublishDraft(publishedAt);
        await _repository.UpdateAsync(item, autoSave: true);
        return Map(item);
    }

    [Authorize(EnglishLearningPlatformAppPermissions.Content.Edit)]
    public async Task<ContentItemDto> CreateRevisionAsync(Guid id, ContentConcurrencyInput input)
    {
        EnsureAuthenticated();
        await _permissionAuthorizer.CheckAsync(EnglishLearningPlatformAppPermissions.Content.Edit);
        var item = await GetItemAsync(id);
        CheckConcurrency(item, input.ConcurrencyStamp);
        item.CreateRevision(GuidGenerator.Create(), GuidGenerator.Create);
        await _repository.UpdateAsync(item, autoSave: true);
        return Map(item);
    }

    [Authorize(EnglishLearningPlatformAppPermissions.Content.Archive)]
    public async Task<ContentItemDto> ArchiveAsync(Guid id, ContentConcurrencyInput input)
    {
        EnsureAuthenticated();
        await _permissionAuthorizer.CheckAsync(EnglishLearningPlatformAppPermissions.Content.Archive);
        var item = await GetItemAsync(id);
        CheckConcurrency(item, input.ConcurrencyStamp);
        item.Archive();
        await _repository.UpdateAsync(item, autoSave: true);
        return Map(item);
    }

    private Task<ContentItem> GetItemAsync(Guid id) => _repository.GetAsync(id, includeDetails: true);

    private static void CheckConcurrency(ContentItem item, string expectedStamp)
    {
        if (!string.Equals(item.ConcurrencyStamp, expectedStamp, StringComparison.Ordinal))
        {
            throw new AbpDbConcurrencyException("The content was changed by another editor.");
        }
    }

    private void EnsureAuthenticated()
    {
        if (!CurrentUser.IsAuthenticated)
        {
            throw new AbpAuthorizationException("Authentication is required for content administration.");
        }
    }

    private static ContentItemDto Map(ContentItem item) => new()
    {
        Id = item.Id,
        Type = item.Type,
        Status = item.Status,
        ChangeSequence = item.ChangeSequence,
        ConcurrencyStamp = item.ConcurrencyStamp,
        CreationTime = item.CreationTime,
        CreatorId = item.CreatorId,
        LastModificationTime = item.LastModificationTime,
        LastModifierId = item.LastModifierId,
        IsDeleted = item.IsDeleted,
        DeleterId = item.DeleterId,
        DeletionTime = item.DeletionTime,
        Versions = item.Versions.OrderBy(x => x.VersionNumber).Select(Map).ToList()
    };

    private static ContentVersionDto Map(ContentVersion version) => new()
    {
        Id = version.Id,
        VersionNumber = version.VersionNumber,
        Title = version.Title,
        Lifecycle = version.Lifecycle,
        PublishedAt = version.PublishedAt
    };

    private static PublishedContentVersionDto MapPublished(ContentItem item, ContentVersion version) => new()
    {
        Id = version.Id,
        ContentItemId = item.Id,
        Type = item.Type,
        VersionNumber = version.VersionNumber,
        Title = version.Title,
        PublishedAt = version.PublishedAt!.Value
    };
}
