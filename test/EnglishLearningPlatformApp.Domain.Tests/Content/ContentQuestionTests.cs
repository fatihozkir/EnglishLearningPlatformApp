using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace EnglishLearningPlatformApp.Content;

public class ContentQuestionTests
{
    [Fact]
    public void All_Required_Question_Types_Should_Be_Ordered()
    {
        var item = CreateItem(out var sectionId);
        foreach (var type in Enum.GetValues<QuestionType>())
        {
            item.AddQuestion(
                sectionId,
                Guid.NewGuid(),
                type,
                $"Prompt {type}",
                "{\"value\":true}",
                Options(type),
                Guid.NewGuid);
        }

        var questions = item.Versions.Single().Sections.Single().Questions.OrderBy(x => x.Position).ToList();
        questions.Count.ShouldBe(7);
        questions.Select(x => x.Position).ShouldBe(Enumerable.Range(1, 7));
        questions.Select(x => x.Type).ShouldBe(Enum.GetValues<QuestionType>());
    }

    [Fact]
    public void Invalid_Answer_Json_And_Published_Mutation_Should_Fail()
    {
        var item = CreateItem(out var sectionId);
        Should.Throw<BusinessException>(() => item.AddQuestion(
                sectionId, Guid.NewGuid(), QuestionType.TrueFalse, "Prompt", "not-json", [], Guid.NewGuid))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.QuestionAnswerDefinitionInvalid);

        item.PublishDraft(DateTime.UtcNow);
        Should.Throw<BusinessException>(() => item.AddQuestion(
                sectionId, Guid.NewGuid(), QuestionType.TrueFalse, "Prompt", "true", [], Guid.NewGuid))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentDraftMissing);
    }

    [Fact]
    public void Undefined_Question_Type_Should_Fail()
    {
        var item = CreateItem(out var sectionId);

        Should.Throw<BusinessException>(() => item.AddQuestion(
                sectionId, Guid.NewGuid(), (QuestionType)99, "Prompt", "true", [], Guid.NewGuid))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.QuestionTypeInvalid);
    }

    [Fact]
    public void Revision_Should_Deep_Copy_Question_Options_And_Answer()
    {
        var item = CreateItem(out var sectionId);
        var original = item.AddQuestion(
            sectionId,
            Guid.NewGuid(),
            QuestionType.Matching,
            "Match",
            "{\"pairs\":[[\"a\",\"b\"]]}",
            [new QuestionOptionValue("a", "b")],
            Guid.NewGuid);
        item.PublishDraft(DateTime.UtcNow);

        var revision = item.CreateRevision(Guid.NewGuid(), Guid.NewGuid);
        var copy = revision.Sections.Single().Questions.Single();

        copy.Id.ShouldNotBe(original.Id);
        copy.Options.Single().Id.ShouldNotBe(original.Options.Single().Id);
        copy.Prompt.ShouldBe(original.Prompt);
        copy.AnswerDefinitionJson.ShouldBe(original.AnswerDefinitionJson);
        copy.Options.Single().MatchText.ShouldBe("b");
    }

    private static ContentItem CreateItem(out Guid sectionId)
    {
        var item = new ContentItem(Guid.NewGuid(), Guid.NewGuid(), ContentType.Reading, Guid.NewGuid(), "Questions");
        sectionId = Guid.NewGuid();
        item.AddSection(sectionId, "Section", "Passage");
        return item;
    }

    private static IReadOnlyList<QuestionOptionValue> Options(QuestionType type) =>
        type is QuestionType.SingleChoice or QuestionType.MultipleSelect or QuestionType.Ordering
            ? [new("A"), new("B")]
            : type == QuestionType.Matching
                ? [new("A", "One"), new("B", "Two")]
                : [];
}
