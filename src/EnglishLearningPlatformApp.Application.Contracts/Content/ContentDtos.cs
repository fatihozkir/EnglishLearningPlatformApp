using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EnglishLearningPlatformApp.Content;

public class ContentItemDto : FullAuditedEntityDto<Guid>
{
    public ContentType Type { get; set; }
    public ContentItemStatus Status { get; set; }
    public int ChangeSequence { get; set; }
    public string ConcurrencyStamp { get; set; } = string.Empty;
    public IReadOnlyList<ContentVersionDto> Versions { get; set; } = [];
}

public class ContentVersionDto : EntityDto<Guid>
{
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public ContentLifecycleStatus Lifecycle { get; set; }
    public DateTime? PublishedAt { get; set; }
    public IReadOnlyList<ContentSectionDto> Sections { get; set; } = [];
}

public class ContentSectionDto : EntityDto<Guid>
{
    public int Position { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public IReadOnlyList<ContentQuestionDto> Questions { get; set; } = [];
}

public class ContentQuestionDto : EntityDto<Guid>
{
    public int Position { get; set; }
    public QuestionType Type { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string AnswerDefinitionJson { get; set; } = string.Empty;
    public IReadOnlyList<ContentQuestionOptionDto> Options { get; set; } = [];
}

public class ContentQuestionOptionDto : EntityDto<Guid>
{
    public int Position { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? MatchText { get; set; }
}

public class PublishedContentSectionDto
{
    public int Position { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public IReadOnlyList<PublishedContentQuestionDto> Questions { get; set; } = [];
}

public class PublishedContentQuestionDto
{
    public int Position { get; set; }
    public QuestionType Type { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public IReadOnlyList<PublishedContentQuestionOptionDto> Options { get; set; } = [];
    public IReadOnlyList<string> MatchChoices { get; set; } = [];
}

public class PublishedContentQuestionOptionDto
{
    public int Position { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class PublishedContentVersionDto : EntityDto<Guid>
{
    public Guid ContentItemId { get; set; }
    public ContentType Type { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public IReadOnlyList<PublishedContentSectionDto> Sections { get; set; } = [];
}
