using Volo.Abp.Reflection;

namespace EnglishLearningPlatformApp.Reading.Permissions;

public class ReadingPermissions
{
    public const string GroupName = "Reading";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ReadingPermissions));
    }
}
