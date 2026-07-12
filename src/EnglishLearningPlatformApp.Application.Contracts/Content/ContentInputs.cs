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
