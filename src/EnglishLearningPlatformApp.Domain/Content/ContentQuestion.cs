using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace EnglishLearningPlatformApp.Content;

public class ContentQuestion : Entity<Guid>, IMultiTenant
{
    private readonly List<ContentQuestionOption> _options = [];

    public Guid? TenantId { get; private set; }
    public Guid ContentSectionId { get; private set; }
    public int Position { get; private set; }
    public QuestionType Type { get; private set; }
    public string Prompt { get; private set; } = string.Empty;
    public string AnswerDefinitionJson { get; private set; } = string.Empty;
    public IReadOnlyCollection<ContentQuestionOption> Options => _options.AsReadOnly();

    protected ContentQuestion() { }

    internal ContentQuestion(
        Guid id,
        Guid sectionId,
        Guid? tenantId,
        int position,
        QuestionType type,
        string prompt,
        string answerDefinitionJson,
        IReadOnlyList<QuestionOptionValue> options,
        Func<Guid> optionIdGenerator) : base(id)
    {
        ContentSectionId = sectionId;
        TenantId = tenantId;
        Position = position;
        SetDefinition(type, prompt, answerDefinitionJson, options, optionIdGenerator);
    }

    internal void Update(
        QuestionType type,
        string prompt,
        string answerDefinitionJson,
        IReadOnlyList<QuestionOptionValue> options,
        Func<Guid> optionIdGenerator) =>
        SetDefinition(type, prompt, answerDefinitionJson, options, optionIdGenerator);

    internal void MoveTo(int position) => Position = position;

    internal ContentQuestion Copy(Guid questionId, Guid sectionId, Func<Guid> idGenerator)
    {
        var optionValues = _options.OrderBy(x => x.Position)
            .Select(x => new QuestionOptionValue(x.Text, x.MatchText)).ToList();
        return new ContentQuestion(
            questionId,
            sectionId,
            TenantId,
            Position,
            Type,
            Prompt,
            AnswerDefinitionJson,
            optionValues,
            idGenerator);
    }

    private static string ValidateAnswerJson(string value)
    {
        var checkedValue = Check.NotNullOrWhiteSpace(
            value,
            nameof(value),
            ContentConsts.MaxQuestionAnswerJsonLength);
        try
        {
            using var document = JsonDocument.Parse(checkedValue);
            return checkedValue;
        }
        catch (JsonException exception)
        {
            throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.QuestionAnswerDefinitionInvalid)
                .WithData("JsonError", exception.Message);
        }
    }

    private void SetDefinition(
        QuestionType type,
        string prompt,
        string answerDefinitionJson,
        IReadOnlyList<QuestionOptionValue> options,
        Func<Guid> optionIdGenerator)
    {
        if (!Enum.IsDefined(type))
        {
            throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.QuestionTypeInvalid)
                .WithData(nameof(type), (int)type);
        }

        ValidateStructure(type, options);

        var checkedPrompt = Check.NotNullOrWhiteSpace(prompt, nameof(prompt), ContentConsts.MaxQuestionPromptLength);
        var checkedAnswer = ValidateAnswerJson(answerDefinitionJson);
        var replacements = options.Select((value, index) =>
            new ContentQuestionOption(optionIdGenerator(), Id, TenantId, index + 1, value)).ToList();

        Type = type;
        Prompt = checkedPrompt;
        AnswerDefinitionJson = checkedAnswer;
        _options.Clear();
        _options.AddRange(replacements);
    }

    private static void ValidateStructure(QuestionType type, IReadOnlyList<QuestionOptionValue> options)
    {
        var requiresOptions = type is QuestionType.SingleChoice or QuestionType.MultipleSelect
            or QuestionType.Ordering or QuestionType.Matching;
        if ((requiresOptions && options.Count < 2) || (!requiresOptions && options.Count != 0) ||
            (type == QuestionType.Matching && options.Any(x => string.IsNullOrWhiteSpace(x.MatchText))))
        {
            throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.QuestionStructureInvalid)
                .WithData(nameof(type), type)
                .WithData("OptionCount", options.Count);
        }
    }
}
