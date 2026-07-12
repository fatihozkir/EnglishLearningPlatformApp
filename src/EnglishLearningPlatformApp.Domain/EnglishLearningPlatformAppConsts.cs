using Volo.Abp.Identity;

namespace EnglishLearningPlatformApp;

public static class EnglishLearningPlatformAppConsts
{
    public const string DbTablePrefix = "App";
    public const string? DbSchema = null;
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordConfigurationName = "Seed:AdminPassword";
}
