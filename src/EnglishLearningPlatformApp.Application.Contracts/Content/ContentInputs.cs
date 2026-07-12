using System.ComponentModel.DataAnnotations;

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
