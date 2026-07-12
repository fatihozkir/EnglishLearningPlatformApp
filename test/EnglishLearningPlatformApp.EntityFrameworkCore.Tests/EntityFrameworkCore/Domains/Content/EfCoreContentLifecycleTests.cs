using System;
using System.Linq;
using System.Threading.Tasks;
using EnglishLearningPlatformApp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace EnglishLearningPlatformApp.Content;

[Collection(EnglishLearningPlatformAppTestConsts.CollectionDefinitionName)]
public class EfCoreContentLifecycleTests : EnglishLearningPlatformAppEntityFrameworkCoreTestBase
{
    private readonly IRepository<ContentItem, Guid> _repository;
    private readonly IRepository<ContentVersion, Guid> _versionRepository;
    private readonly ICurrentTenant _currentTenant;

    public EfCoreContentLifecycleTests()
    {
        _repository = GetRequiredService<IRepository<ContentItem, Guid>>();
        _versionRepository = GetRequiredService<IRepository<ContentVersion, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Aggregate_Should_Persist_Versions_And_Preserve_Published_Snapshot()
    {
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var item = new ContentItem(itemId, tenantId, ContentType.Reading, Guid.NewGuid(), "Version one");
                item.PublishDraft(new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc));
                item.CreateRevision(Guid.NewGuid());
                item.UpdateDraft("Version two");
                await _repository.InsertAsync(item);
            });

            var reloaded = await WithUnitOfWorkAsync(() => _repository.GetAsync(itemId, includeDetails: true));
            reloaded.Versions.Count.ShouldBe(2);
            reloaded.Versions.OrderBy(x => x.VersionNumber).Select(x => x.Title)
                .ShouldBe(new[] { "Version one", "Version two" });
        }
    }

    [Fact]
    public async Task Tenant_And_Host_Filters_Should_Hide_Other_Tenant_Content()
    {
        var tenantOne = Guid.NewGuid();
        var tenantTwo = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        using (_currentTenant.Change(tenantOne))
        {
            await WithUnitOfWorkAsync(() => _repository.InsertAsync(
                new ContentItem(itemId, tenantOne, ContentType.Listening, Guid.NewGuid(), "Tenant one")));
            (await _repository.FindAsync(itemId)).ShouldNotBeNull();
        }

        using (_currentTenant.Change(tenantTwo))
        {
            (await _repository.FindAsync(itemId)).ShouldBeNull();
            (await _versionRepository.GetCountAsync()).ShouldBe(0);
        }

        using (_currentTenant.Change(null))
        {
            (await _repository.FindAsync(itemId)).ShouldBeNull();
            (await _versionRepository.GetCountAsync()).ShouldBe(0);
        }
    }

    [Fact]
    public async Task Database_Should_Reject_A_Second_Draft_For_The_Same_Item()
    {
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() => _repository.InsertAsync(
                new ContentItem(itemId, tenantId, ContentType.Reading, Guid.NewGuid(), "Draft one")));

            await Should.ThrowAsync<DbUpdateException>(() => WithUnitOfWorkAsync(() =>
                _versionRepository.InsertAsync(
                    new ContentVersion(Guid.NewGuid(), itemId, tenantId, 2, "Draft two"), autoSave: true)));
        }
    }

    [Fact]
    public async Task Stale_Concurrent_Draft_Edit_Should_Be_Rejected()
    {
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() => _repository.InsertAsync(
                new ContentItem(itemId, tenantId, ContentType.Reading, Guid.NewGuid(), "Original")));
        }

        using (_currentTenant.Change(tenantId))
        {
            var staleEditor = await WithUnitOfWorkAsync(() => _repository.GetAsync(itemId, includeDetails: true));

            await WithUnitOfWorkAsync(async () =>
            {
                var firstEditor = await _repository.GetAsync(itemId, includeDetails: true);
                firstEditor.UpdateDraft("First editor");
                await _repository.UpdateAsync(firstEditor, autoSave: true);
            });

            staleEditor.UpdateDraft("Stale editor");
            await Should.ThrowAsync<AbpDbConcurrencyException>(() => WithUnitOfWorkAsync(() =>
                _repository.UpdateAsync(staleEditor, autoSave: true)));
        }
    }
}
