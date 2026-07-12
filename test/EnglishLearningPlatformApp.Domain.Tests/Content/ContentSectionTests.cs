using System;
using System.Linq;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace EnglishLearningPlatformApp.Content;

public class ContentSectionTests
{
    [Fact]
    public void Draft_Sections_Should_Support_Contiguous_Order_And_Mutation()
    {
        var item = CreateItem();
        var first = item.AddSection(Guid.NewGuid(), "One", "First body");
        var second = item.AddSection(Guid.NewGuid(), "Two", "Second body");
        var third = item.AddSection(Guid.NewGuid(), "Three", "Third body");

        item.UpdateSection(second.Id, "Two updated", "Second updated");
        item.ReorderSections(new[] { third.Id, first.Id, second.Id });
        item.RemoveSection(first.Id);

        var ordered = item.Versions.Single().Sections.OrderBy(x => x.Position).ToList();
        ordered.Select(x => x.Position).ShouldBe(new[] { 1, 2 });
        ordered.Select(x => x.Heading).ShouldBe(new[] { "Three", "Two updated" });
    }

    [Fact]
    public void Published_Sections_Should_Be_Immutable_And_Revision_Should_Deep_Copy()
    {
        var item = CreateItem();
        var sourceSection = item.AddSection(Guid.NewGuid(), "Passage", "Original body");
        item.PublishDraft(DateTime.UtcNow);

        Should.Throw<BusinessException>(() => item.UpdateSection(sourceSection.Id, "Changed", "Changed"))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentDraftMissing);

        var revision = item.CreateRevision(Guid.NewGuid(), Guid.NewGuid);
        var copiedSection = revision.Sections.Single();
        copiedSection.Id.ShouldNotBe(sourceSection.Id);
        copiedSection.Heading.ShouldBe(sourceSection.Heading);
        copiedSection.Body.ShouldBe(sourceSection.Body);

        item.UpdateSection(copiedSection.Id, "Revision", "Revision body");
        sourceSection.Heading.ShouldBe("Passage");
        sourceSection.Body.ShouldBe("Original body");
    }

    [Fact]
    public void Invalid_Reorder_Should_Fail()
    {
        var item = CreateItem();
        var section = item.AddSection(Guid.NewGuid(), "One", "Body");

        Should.Throw<BusinessException>(() => item.ReorderSections(new[] { section.Id, Guid.NewGuid() }))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentSectionOrderInvalid);
    }

    private static ContentItem CreateItem() =>
        new(Guid.NewGuid(), Guid.NewGuid(), ContentType.Reading, Guid.NewGuid(), "Content");
}
