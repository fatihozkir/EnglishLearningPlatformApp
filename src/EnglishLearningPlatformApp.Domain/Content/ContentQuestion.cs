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
        if (!Enum.IsDefined(type))
        {
            throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.QuestionTypeInvalid)
                .WithData(nameof(type), (int)type);
        }

        Type = type;
        Prompt = Check.NotNullOrWhiteSpace(prompt, nameof(prompt), ContentConsts.MaxQuestionPromptLength);
        AnswerDefinitionJson = ValidateAnswerJson(answerDefinitionJson);

        for (var index = 0; index < options.Count; index++)
        {
            _options.Add(new ContentQuestionOption(optionIdGenerator(), id, tenantId, index + 1, options[index]));
        }
    }

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
}
