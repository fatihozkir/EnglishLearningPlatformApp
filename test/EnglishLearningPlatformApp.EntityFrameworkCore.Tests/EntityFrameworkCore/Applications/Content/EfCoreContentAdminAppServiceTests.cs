using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EnglishLearningPlatformApp.EntityFrameworkCore;
using EnglishLearningPlatformApp.Security;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Data;
using EnglishLearningPlatformApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace EnglishLearningPlatformApp.Content;

[Collection(EnglishLearningPlatformAppTestConsts.CollectionDefinitionName)]
public class EfCoreContentAdminAppServiceTests : EnglishLearningPlatformAppEntityFrameworkCoreTestBase
{
    private readonly IContentAdminAppService _service;
    private readonly ICurrentTenant _currentTenant;
    private readonly FakeCurrentPrincipalAccessor _principalAccessor;
    private readonly ConfigurableContentPermissionAuthorizer _authorizationService;

    public EfCoreContentAdminAppServiceTests()
    {
        _service = GetRequiredService<IContentAdminAppService>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _principalAccessor = (FakeCurrentPrincipalAccessor)GetRequiredService<ICurrentPrincipalAccessor>();
        _authorizationService = (ConfigurableContentPermissionAuthorizer)GetRequiredService<IContentPermissionAuthorizer>();
    }

    [Fact]
    public async Task Administrator_Should_Complete_Explicit_Lifecycle()
    {
        using var tenant = _currentTenant.Change(Guid.NewGuid());
        var created = await _service.CreateAsync(new CreateContentInput
        {
            Type = ContentType.Reading,
            Title = "Draft one"
        });

        var edited = await _service.UpdateDraftAsync(created.Id, new UpdateContentDraftInput
        {
            Title = "Published one",
            ConcurrencyStamp = created.ConcurrencyStamp
        });
        edited.Versions.Single().Title.ShouldBe("Published one");

        var publishedOne = await _service.PublishAsync(created.Id, Stamp(edited));
        var versionOne = publishedOne.Versions.Single(x => x.VersionNumber == 1);
        versionOne.Title.ShouldBe("Published one");

        var revision = await _service.CreateRevisionAsync(created.Id, Stamp(publishedOne));
        revision.Versions.Count.ShouldBe(2);
        var editedTwo = await _service.UpdateDraftAsync(created.Id, new UpdateContentDraftInput
        {
            Title = "Published two",
            ConcurrencyStamp = revision.ConcurrencyStamp
        });
        var publishedTwo = await _service.PublishAsync(created.Id, Stamp(editedTwo));
        publishedTwo.Versions.Single(x => x.VersionNumber == 2).Title.ShouldBe("Published two");

        var archived = await _service.ArchiveAsync(created.Id, Stamp(publishedTwo));
        archived.Status.ShouldBe(ContentItemStatus.Archived);
        (await _service.GetPublishedAsync(created.Id)).VersionNumber.ShouldBe(2);
    }

    [Fact]
    public async Task Anonymous_Create_Should_Be_Denied_Behaviorally()
    {
        using var tenant = _currentTenant.Change(Guid.NewGuid());
        using (_principalAccessor.ChangeTo(new ClaimsPrincipal(new ClaimsIdentity())))
        {
            await Should.ThrowAsync<AbpAuthorizationException>(() => _service.CreateAsync(new CreateContentInput
            {
                Type = ContentType.Listening,
                Title = "Denied"
            }));
        }
    }

    [Fact]
    public async Task Authenticated_User_Without_Content_Permissions_Should_Be_Denied()
    {
        using var tenant = _currentTenant.Change(Guid.NewGuid());
        var created = await _service.CreateAsync(new CreateContentInput { Type = ContentType.Reading, Title = "Protected" });

        using (_authorizationService.DenyAll())
        {
            await Should.ThrowAsync<AbpAuthorizationException>(() => _service.CreateAsync(
                new CreateContentInput { Type = ContentType.Reading, Title = "Denied create" }));
            await Should.ThrowAsync<AbpAuthorizationException>(() => _service.UpdateDraftAsync(
                created.Id, new UpdateContentDraftInput { Title = "Denied edit", ConcurrencyStamp = created.ConcurrencyStamp }));
            await Should.ThrowAsync<AbpAuthorizationException>(() => _service.PublishAsync(created.Id, Stamp(created)));
            await Should.ThrowAsync<AbpAuthorizationException>(() => _service.CreateRevisionAsync(created.Id, Stamp(created)));
            await Should.ThrowAsync<AbpAuthorizationException>(() => _service.ArchiveAsync(created.Id, Stamp(created)));
        }
    }

    [Fact]
    public async Task Stale_Application_Command_Should_Be_Rejected()
    {
        using var tenant = _currentTenant.Change(Guid.NewGuid());
        var created = await _service.CreateAsync(new CreateContentInput { Type = ContentType.Reading, Title = "Original" });
        var firstUpdate = await _service.UpdateDraftAsync(created.Id, new UpdateContentDraftInput
        {
            Title = "First update",
            ConcurrencyStamp = created.ConcurrencyStamp
        });

        await Should.ThrowAsync<AbpDbConcurrencyException>(() => _service.UpdateDraftAsync(created.Id,
            new UpdateContentDraftInput { Title = "Stale overwrite", ConcurrencyStamp = created.ConcurrencyStamp }));

        (await _service.GetAsync(created.Id)).Versions.Single().Title.ShouldBe("First update");
        firstUpdate.ConcurrencyStamp.ShouldNotBe(created.ConcurrencyStamp);
    }

    [Fact]
    public async Task Cross_Tenant_Read_Should_Not_Resolve_Content()
    {
        var tenantOne = Guid.NewGuid();
        var tenantTwo = Guid.NewGuid();
        Guid itemId;

        using (_currentTenant.Change(tenantOne))
        {
            itemId = (await _service.CreateAsync(new CreateContentInput
            {
                Type = ContentType.Listening,
                Title = "Tenant one"
            })).Id;
        }

        using (_currentTenant.Change(tenantTwo))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _service.GetAsync(itemId));
            await Should.ThrowAsync<EntityNotFoundException>(() => _service.UpdateDraftAsync(itemId,
                new UpdateContentDraftInput { Title = "Cross-tenant mutation", ConcurrencyStamp = "irrelevant" }));
        }

        using (_currentTenant.Change(tenantOne))
        {
            (await _service.GetAsync(itemId)).Versions.Single().Title.ShouldBe("Tenant one");
        }
    }

    [Fact]
    public async Task Host_Context_Should_Not_Create_Content()
    {
        using (_currentTenant.Change(null))
        {
            await Should.ThrowAsync<AbpAuthorizationException>(() => _service.CreateAsync(new CreateContentInput
            {
                Type = ContentType.Reading,
                Title = "Host content"
            }));
        }
    }

    [Fact]
    public void Published_Contract_Should_Not_Expose_Answer_Or_Grading_Fields()
    {
        var forbiddenFragments = new[] { "answer", "correct", "solution", "grading", "score" };
        var propertyNames = typeof(PublishedContentVersionDto).GetProperties().Select(x => x.Name).ToList();

        propertyNames.ShouldAllBe(name =>
            forbiddenFragments.All(fragment => !name.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Lifecycle_Commands_Should_Declare_Exact_Permissions()
    {
        typeof(ContentAdminAppService).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Policy.ShouldBe(EnglishLearningPlatformAppPermissions.Content.Default);

        AssertPolicy(nameof(ContentAdminAppService.CreateAsync), EnglishLearningPlatformAppPermissions.Content.Create);
        AssertPolicy(nameof(ContentAdminAppService.UpdateDraftAsync), EnglishLearningPlatformAppPermissions.Content.Edit);
        AssertPolicy(nameof(ContentAdminAppService.CreateRevisionAsync), EnglishLearningPlatformAppPermissions.Content.Edit);
        AssertPolicy(nameof(ContentAdminAppService.PublishAsync), EnglishLearningPlatformAppPermissions.Content.Publish);
        AssertPolicy(nameof(ContentAdminAppService.ArchiveAsync), EnglishLearningPlatformAppPermissions.Content.Archive);
    }

    private static void AssertPolicy(string methodName, string expectedPolicy)
    {
        typeof(ContentAdminAppService).GetMethod(methodName)!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Policy.ShouldBe(expectedPolicy);
    }

    private static ContentConcurrencyInput Stamp(ContentItemDto item) =>
        new() { ConcurrencyStamp = item.ConcurrencyStamp };
}
