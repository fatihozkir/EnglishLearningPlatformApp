using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text.Json;
using EnglishLearningPlatformApp.EntityFrameworkCore;
using EnglishLearningPlatformApp.Security;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.Data;
using Volo.Abp.Validation;
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
        var propertyNames = new[]
            {
                typeof(PublishedContentVersionDto), typeof(PublishedContentSectionDto),
                typeof(PublishedContentQuestionDto), typeof(PublishedContentQuestionOptionDto)
            }
            .SelectMany(type => type.GetProperties()).Select(property => property.Name).ToList();

        typeof(PublishedContentSectionDto).GetProperties().Select(x => x.Name)
            .ShouldBe(new[] { "Position", "Heading", "Body", "Questions" });

        typeof(PublishedContentQuestionDto).GetProperties().Select(x => x.Name)
            .ShouldBe(new[] { "Position", "Type", "Prompt", "Options", "MatchChoices" });

        typeof(PublishedContentQuestionOptionDto).GetProperties().Select(x => x.Name)
            .ShouldBe(new[] { "Position", "Text" });

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
        AssertPolicy(nameof(ContentAdminAppService.AddSectionAsync), EnglishLearningPlatformAppPermissions.Content.Edit);
        AssertPolicy(nameof(ContentAdminAppService.UpdateSectionAsync), EnglishLearningPlatformAppPermissions.Content.Edit);
        AssertPolicy(nameof(ContentAdminAppService.RemoveSectionAsync), EnglishLearningPlatformAppPermissions.Content.Edit);
        AssertPolicy(nameof(ContentAdminAppService.ReorderSectionsAsync), EnglishLearningPlatformAppPermissions.Content.Edit);
        AssertPolicy(nameof(ContentAdminAppService.AddQuestionAsync), EnglishLearningPlatformAppPermissions.Content.Edit);
    }

    [Fact]
    public async Task Administrator_Should_Manage_And_Publish_Ordered_Sections()
    {
        using var tenant = _currentTenant.Change(Guid.NewGuid());
        var item = await _service.CreateAsync(new CreateContentInput { Type = ContentType.Reading, Title = "Passages" });
        item = await AddSection(item, "One", "Body one");
        item = await AddSection(item, "Two", "Body two");
        item = await AddSection(item, "Three", "Body three");

        var sections = item.Versions.Single().Sections;
        var second = sections.Single(x => x.Heading == "Two");
        item = await _service.UpdateSectionAsync(item.Id, second.Id, new UpdateContentSectionInput
        {
            Heading = "Two updated",
            Body = "Body two updated",
            ConcurrencyStamp = item.ConcurrencyStamp
        });

        var reversedIds = item.Versions.Single().Sections.OrderByDescending(x => x.Position).Select(x => x.Id).ToList();
        item = await _service.ReorderSectionsAsync(item.Id, new ReorderContentSectionsInput
        {
            SectionIds = reversedIds,
            ConcurrencyStamp = item.ConcurrencyStamp
        });
        var firstAfterReverse = item.Versions.Single().Sections.Single(x => x.Position == 1);
        item = await _service.RemoveSectionAsync(item.Id, firstAfterReverse.Id, Stamp(item));

        var published = await _service.PublishAsync(item.Id, Stamp(item));
        var presentation = await _service.GetPublishedAsync(item.Id);
        presentation.Sections.Select(x => x.Position).ShouldBe(new[] { 1, 2 });
        presentation.Sections.Select(x => x.Heading).ShouldBe(new[] { "Two updated", "One" });
        published.Status.ShouldBe(ContentItemStatus.Published);
    }

    [Fact]
    public async Task Section_Commands_Should_Enforce_Permission_Concurrency_And_Tenant()
    {
        var tenantOne = Guid.NewGuid();
        var tenantTwo = Guid.NewGuid();
        ContentItemDto item;
        using (_currentTenant.Change(tenantOne))
        {
            item = await _service.CreateAsync(new CreateContentInput { Type = ContentType.Reading, Title = "Protected sections" });

            using (_authorizationService.DenyAll())
            {
                await Should.ThrowAsync<AbpAuthorizationException>(() => _service.AddSectionAsync(item.Id,
                    new AddContentSectionInput { Heading = "Denied", Body = "Denied", ConcurrencyStamp = item.ConcurrencyStamp }));
            }

            var staleStamp = item.ConcurrencyStamp;
            item = await AddSection(item, "Accepted", "Accepted body");
            await Should.ThrowAsync<AbpDbConcurrencyException>(() => _service.AddSectionAsync(item.Id,
                new AddContentSectionInput { Heading = "Stale", Body = "Stale", ConcurrencyStamp = staleStamp }));
        }

        using (_currentTenant.Change(tenantTwo))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _service.AddSectionAsync(item.Id,
                new AddContentSectionInput { Heading = "Cross tenant", Body = "Cross tenant", ConcurrencyStamp = item.ConcurrencyStamp }));
        }

        using (_currentTenant.Change(tenantOne))
        {
            (await _service.GetAsync(item.Id)).Versions.Single().Sections.Select(x => x.Heading)
                .ShouldBe(new[] { "Accepted" });
        }
    }

    [Fact]
    public async Task Administrator_Should_Add_All_Question_Types_Without_Published_Answer_Leakage()
    {
        using var tenant = _currentTenant.Change(Guid.NewGuid());
        var item = await _service.CreateAsync(new CreateContentInput { Type = ContentType.Reading, Title = "Question set" });
        item = await AddSection(item, "Passage", "Read this");
        var sectionId = item.Versions.Single().Sections.Single().Id;

        foreach (var type in Enum.GetValues<QuestionType>())
        {
            item = await _service.AddQuestionAsync(item.Id, new AddContentQuestionInput
            {
                SectionId = sectionId,
                Type = type,
                Prompt = $"Prompt {type}",
                AnswerDefinitionJson = $"{{\"type\":\"{type}\",\"secret\":true}}",
                Options = type == QuestionType.Matching
                    ?
                    [
                        new ContentQuestionOptionInput { Text = "A", MatchText = "Two" },
                        new ContentQuestionOptionInput { Text = "B", MatchText = "One" }
                    ]
                    : [new ContentQuestionOptionInput { Text = "A" }],
                ConcurrencyStamp = item.ConcurrencyStamp
            });
        }

        var adminQuestions = item.Versions.Single().Sections.Single().Questions;
        adminQuestions.Select(x => x.Type).ShouldBe(Enum.GetValues<QuestionType>());
        adminQuestions.ShouldAllBe(x => x.AnswerDefinitionJson.Contains("secret", StringComparison.Ordinal));

        item = await _service.PublishAsync(item.Id, Stamp(item));
        var published = await _service.GetPublishedAsync(item.Id);
        published.Sections.Single().Questions.Select(x => x.Type).ShouldBe(Enum.GetValues<QuestionType>());
        var json = JsonSerializer.Serialize(published);
        json.ShouldNotContain("AnswerDefinition", Case.Insensitive);
        json.ShouldNotContain("secret", Case.Insensitive);
        var matching = published.Sections.Single().Questions.Single(x => x.Type == QuestionType.Matching);
        matching.Options.Select(x => x.Text).ShouldBe(new[] { "A", "B" });
        matching.MatchChoices.ShouldBe(new[] { "One", "Two" });
    }

    [Fact]
    public async Task Add_Question_Should_Enforce_Permission_Concurrency_And_Tenant()
    {
        var tenantOne = Guid.NewGuid();
        var tenantTwo = Guid.NewGuid();
        ContentItemDto item;
        Guid sectionId;
        using (_currentTenant.Change(tenantOne))
        {
            item = await _service.CreateAsync(new CreateContentInput { Type = ContentType.Reading, Title = "Protected questions" });
            item = await AddSection(item, "Passage", "Body");
            sectionId = item.Versions.Single().Sections.Single().Id;
            var denied = QuestionInput(sectionId, item.ConcurrencyStamp);
            using (_authorizationService.DenyAll())
            {
                await Should.ThrowAsync<AbpAuthorizationException>(() => _service.AddQuestionAsync(item.Id, denied));
            }

            var staleStamp = item.ConcurrencyStamp;
            item = await _service.AddQuestionAsync(item.Id, QuestionInput(sectionId, item.ConcurrencyStamp));
            await Should.ThrowAsync<AbpDbConcurrencyException>(() =>
                _service.AddQuestionAsync(item.Id, QuestionInput(sectionId, staleStamp)));
        }

        using (_currentTenant.Change(tenantTwo))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _service.AddQuestionAsync(item.Id, QuestionInput(sectionId, item.ConcurrencyStamp)));
        }

        using (_currentTenant.Change(tenantOne))
        {
            (await _service.GetAsync(item.Id)).Versions.Single().Sections.Single().Questions.Count.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Add_Question_Should_Reject_Null_Options()
    {
        using var tenant = _currentTenant.Change(Guid.NewGuid());
        var item = await _service.CreateAsync(new CreateContentInput { Type = ContentType.Reading, Title = "Validation" });
        item = await AddSection(item, "Passage", "Body");

        await Should.ThrowAsync<AbpValidationException>(() => _service.AddQuestionAsync(item.Id, new AddContentQuestionInput
        {
            SectionId = item.Versions.Single().Sections.Single().Id,
            Type = QuestionType.TrueFalse,
            Prompt = "Prompt",
            AnswerDefinitionJson = "true",
            Options = null!,
            ConcurrencyStamp = item.ConcurrencyStamp
        }));
    }

    private static void AssertPolicy(string methodName, string expectedPolicy)
    {
        typeof(ContentAdminAppService).GetMethod(methodName)!
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>().Single().Policy.ShouldBe(expectedPolicy);
    }

    private static ContentConcurrencyInput Stamp(ContentItemDto item) =>
        new() { ConcurrencyStamp = item.ConcurrencyStamp };

    private async Task<ContentItemDto> AddSection(ContentItemDto item, string heading, string body) =>
        await _service.AddSectionAsync(item.Id, new AddContentSectionInput
        {
            Heading = heading,
            Body = body,
            ConcurrencyStamp = item.ConcurrencyStamp
        });

    private static AddContentQuestionInput QuestionInput(Guid sectionId, string stamp) => new()
    {
        SectionId = sectionId,
        Type = QuestionType.TrueFalse,
        Prompt = "True?",
        AnswerDefinitionJson = "true",
        ConcurrencyStamp = stamp
    };
}
