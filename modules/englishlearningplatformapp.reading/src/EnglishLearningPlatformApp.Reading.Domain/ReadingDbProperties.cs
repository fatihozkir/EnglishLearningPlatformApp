namespace EnglishLearningPlatformApp.Reading;

public static class ReadingDbProperties
{
    public static string DbTablePrefix { get; set; } = "Reading";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "Reading";
}
