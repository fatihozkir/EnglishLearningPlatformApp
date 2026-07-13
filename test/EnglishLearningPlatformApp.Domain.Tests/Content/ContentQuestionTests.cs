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

    [Fact]
    public void Draft_Questions_Should_Update_Remove_And_Reorder_Atomically()
    {
        var item = CreateItem(out var sectionId);
        var first = item.AddQuestion(sectionId, Guid.NewGuid(), QuestionType.SingleChoice, "First", "1",
            [new("Old A"), new("Old B")], Guid.NewGuid);
        var oldOptionIds = first.Options.Select(x => x.Id).ToList();
        var second = item.AddQuestion(sectionId, Guid.NewGuid(), QuestionType.TrueFalse, "Second", "true", [], Guid.NewGuid);
        var third = item.AddQuestion(sectionId, Guid.NewGuid(), QuestionType.ShortAnswer, "Third", "\"ok\"", [], Guid.NewGuid);

        item.UpdateQuestion(sectionId, first.Id, QuestionType.MultipleSelect, "Updated", "[1,2]",
            [new("New A"), new("New B")], Guid.NewGuid);
        first.Options.Select(x => x.Id).ShouldNotContain(oldOptionIds[0]);
        first.Prompt.ShouldBe("Updated");

        item.ReorderQuestions(sectionId, [third.Id, first.Id, second.Id]);
        item.RemoveQuestion(sectionId, first.Id);
        item.Versions.Single().Sections.Single().Questions.OrderBy(x => x.Position).Select(x => x.Id)
            .ShouldBe(new[] { third.Id, second.Id });
        item.Versions.Single().Sections.Single().Questions.Select(x => x.Position).OrderBy(x => x)
            .ShouldBe(new[] { 1, 2 });

        Should.Throw<BusinessException>(() => item.ReorderQuestions(sectionId, [third.Id, third.Id]))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentQuestionOrderInvalid);
        item.Versions.Single().Sections.Single().Questions.OrderBy(x => x.Position).Select(x => x.Id)
            .ShouldBe(new[] { third.Id, second.Id });
    }

    [Fact]
    public void Invalid_Update_Should_Leave_Question_Unchanged()
    {
        var item = CreateItem(out var sectionId);
        var question = item.AddQuestion(sectionId, Guid.NewGuid(), QuestionType.SingleChoice, "Original", "1",
            [new("A"), new("B")], Guid.NewGuid);
        var optionIds = question.Options.Select(x => x.Id).ToList();

        Should.Throw<BusinessException>(() => item.UpdateQuestion(sectionId, question.Id, QuestionType.MultipleSelect,
            "Changed", "bad-json", [new("Replacement")], Guid.NewGuid));

        question.Type.ShouldBe(QuestionType.SingleChoice);
        question.Prompt.ShouldBe("Original");
        question.AnswerDefinitionJson.ShouldBe("1");
        question.Options.Select(x => x.Id).ShouldBe(optionIds);
    }

    [Fact]
    public void Published_Question_Mutations_Should_Fail()
    {
        var item = CreateItem(out var sectionId);
        var question = item.AddQuestion(sectionId, Guid.NewGuid(), QuestionType.TrueFalse, "Prompt", "true", [], Guid.NewGuid);
        item.PublishDraft(DateTime.UtcNow);

        Should.Throw<BusinessException>(() => item.UpdateQuestion(sectionId, question.Id, QuestionType.TrueFalse,
            "Changed", "false", [], Guid.NewGuid)).Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentDraftMissing);
        Should.Throw<BusinessException>(() => item.RemoveQuestion(sectionId, question.Id))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentDraftMissing);
        Should.Throw<BusinessException>(() => item.ReorderQuestions(sectionId, [question.Id]))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentDraftMissing);

        var archived = CreateItem(out var archivedSectionId);
        var archivedQuestion = archived.AddQuestion(archivedSectionId, Guid.NewGuid(), QuestionType.TrueFalse,
            "Prompt", "true", [], Guid.NewGuid);
        archived.Archive();
        Should.Throw<BusinessException>(() => archived.UpdateQuestion(archivedSectionId, archivedQuestion.Id,
            QuestionType.TrueFalse, "Changed", "false", [], Guid.NewGuid))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentArchived);
        Should.Throw<BusinessException>(() => archived.RemoveQuestion(archivedSectionId, archivedQuestion.Id))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentArchived);
        Should.Throw<BusinessException>(() => archived.ReorderQuestions(archivedSectionId, [archivedQuestion.Id]))
            .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.ContentArchived);
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
