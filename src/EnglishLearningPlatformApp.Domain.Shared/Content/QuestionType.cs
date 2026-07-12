namespace EnglishLearningPlatformApp.Content;

public enum QuestionType
{
    SingleChoice = 0,
    MultipleSelect = 1,
    TrueFalse = 2,
    FillBlank = 3,
    ShortAnswer = 4,
    Ordering = 5,
    Matching = 6
}

public sealed record QuestionOptionValue(string Text, string? MatchText = null);
