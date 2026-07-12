using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace EnglishLearningPlatformApp.Content;

public class ContentQuestionOption : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ContentQuestionId { get; private set; }
    public int Position { get; private set; }
    public string Text { get; private set; } = string.Empty;
    public string? MatchText { get; private set; }

    protected ContentQuestionOption() { }

    internal ContentQuestionOption(Guid id, Guid questionId, Guid? tenantId, int position, QuestionOptionValue value)
        : base(id)
    {
        ContentQuestionId = questionId;
        TenantId = tenantId;
        Position = position;
        Text = Check.NotNullOrWhiteSpace(value.Text, nameof(value.Text), ContentConsts.MaxQuestionOptionTextLength);
        MatchText = Check.Length(value.MatchText, nameof(value.MatchText), ContentConsts.MaxQuestionOptionTextLength);
    }
}
