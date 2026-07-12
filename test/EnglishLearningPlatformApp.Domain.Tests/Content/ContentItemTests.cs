using System;
using System.Linq;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace EnglishLearningPlatformApp.Content;

public class ContentItemTests
{
    [Fact]
    public void New_Item_Should_Create_Draft_Version_One()
    {
        var item = CreateItem("First title");

        item.Status.ShouldBe(ContentItemStatus.Draft);
        var version = item.Versions.Single();
        version.VersionNumber.ShouldBe(1);
        version.Title.ShouldBe("First title");
        version.Lifecycle.ShouldBe(ContentLifecycleStatus.Draft);
    }

    [Fact]
    public void Publishing_Should_Freeze_Version_And_Revision_Should_Not_Change_It()
    {
        var item = CreateItem("Version one");
        var publishedAt = new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc);

        item.PublishDraft(publishedAt);
        var published = item.Versions.Single();
        var revision = item.CreateRevision(Guid.NewGuid());
        item.UpdateDraft("Version two");

        published.Title.ShouldBe("Version one");
        published.PublishedAt.ShouldBe(publishedAt);
        published.Lifecycle.ShouldBe(ContentLifecycleStatus.Published);
        revision.VersionNumber.ShouldBe(2);
        revision.Title.ShouldBe("Version two");
    }

    [Fact]
    public void Duplicate_Draft_And_Archived_Changes_Should_Fail()
    {
        var item = CreateItem("Draft");
        Should.Throw<BusinessException>(() => item.CreateRevision(Guid.NewGuid()))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentDraftAlreadyExists);

        item.Archive();
        Should.Throw<BusinessException>(() => item.UpdateDraft("Blocked"))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentArchived);
        Should.Throw<BusinessException>(() => item.PublishDraft(DateTime.UtcNow))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentArchived);
        Should.Throw<BusinessException>(() => item.CreateRevision(Guid.NewGuid()))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentArchived);
    }

    private static ContentItem CreateItem(string title) =>
        new(Guid.NewGuid(), Guid.NewGuid(), ContentType.Reading, Guid.NewGuid(), title);
}
