using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace EnglishLearningPlatformApp.Content;

public class CreateContentInput
{
    public ContentType Type { get; set; }

    [Required]
    [StringLength(ContentConsts.MaxTitleLength)]
    public string Title { get; set; } = string.Empty;
}

public class UpdateContentDraftInput
{
    [Required]
    [StringLength(ContentConsts.MaxTitleLength)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class ContentConcurrencyInput
{
    [Required]
    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class AddContentSectionInput : ContentConcurrencyInput
{
    [StringLength(ContentConsts.MaxSectionHeadingLength)]
    public string Heading { get; set; } = string.Empty;

    [Required]
    [StringLength(ContentConsts.MaxSectionBodyLength)]
    public string Body { get; set; } = string.Empty;
}

public class UpdateContentSectionInput : AddContentSectionInput
{
}

public class ReorderContentSectionsInput : ContentConcurrencyInput
{
    [Required]
    public IReadOnlyList<Guid> SectionIds { get; set; } = [];
}

public class AddContentQuestionInput : ContentConcurrencyInput
{
    public Guid SectionId { get; set; }
    public QuestionType Type { get; set; }

    [Required]
    [StringLength(ContentConsts.MaxQuestionPromptLength)]
    public string Prompt { get; set; } = string.Empty;

    [Required]
    [StringLength(ContentConsts.MaxQuestionAnswerJsonLength)]
    public string AnswerDefinitionJson { get; set; } = string.Empty;

    [Required]
    public IReadOnlyList<ContentQuestionOptionInput> Options { get; set; } = [];
}

public class ContentQuestionOptionInput
{
    [Required]
    [StringLength(ContentConsts.MaxQuestionOptionTextLength)]
    public string Text { get; set; } = string.Empty;

    [StringLength(ContentConsts.MaxQuestionOptionTextLength)]
    public string? MatchText { get; set; }
}

public class UpdateContentQuestionInput : AddContentQuestionInput
{
}

public class ReorderContentQuestionsInput : ContentConcurrencyInput
{
    public Guid SectionId { get; set; }

    [Required]
    public IReadOnlyList<Guid> QuestionIds { get; set; } = [];
}
