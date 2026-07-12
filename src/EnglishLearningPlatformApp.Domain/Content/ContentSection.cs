using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using System.Collections.Generic;
using System.Linq;

namespace EnglishLearningPlatformApp.Content;

public class ContentSection : Entity<Guid>, IMultiTenant
{
    private readonly List<ContentQuestion> _questions = [];
    public Guid? TenantId { get; private set; }
    public Guid ContentVersionId { get; private set; }
    public int Position { get; private set; }
    public string Heading { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public IReadOnlyCollection<ContentQuestion> Questions => _questions.AsReadOnly();

    protected ContentSection() { }

    internal ContentSection(
        Guid id,
        Guid contentVersionId,
        Guid? tenantId,
        int position,
        string heading,
        string body) : base(id)
    {
        ContentVersionId = contentVersionId;
        TenantId = tenantId;
        Position = position;
        SetContent(heading, body);
    }

    internal void Update(string heading, string body) => SetContent(heading, body);

    internal void MoveTo(int position) => Position = position;

    internal ContentQuestion AddQuestion(
        Guid questionId,
        QuestionType type,
        string prompt,
        string answerDefinitionJson,
        IReadOnlyList<QuestionOptionValue> options,
        Func<Guid> optionIdGenerator)
    {
        var question = new ContentQuestion(
            questionId,
            Id,
            TenantId,
            _questions.Count + 1,
            type,
            prompt,
            answerDefinitionJson,
            options,
            optionIdGenerator);
        _questions.Add(question);
        return question;
    }

    internal ContentSection Copy(Guid sectionId, Guid versionId, Func<Guid> idGenerator)
    {
        var copy = new ContentSection(sectionId, versionId, TenantId, Position, Heading, Body);
        foreach (var question in _questions.OrderBy(x => x.Position))
        {
            copy._questions.Add(question.Copy(idGenerator(), sectionId, idGenerator));
        }

        return copy;
    }

    private void SetContent(string heading, string body)
    {
        Heading = Check.Length(heading ?? string.Empty, nameof(heading), ContentConsts.MaxSectionHeadingLength)!;
        Body = Check.NotNullOrWhiteSpace(body, nameof(body), ContentConsts.MaxSectionBodyLength);
    }
}
