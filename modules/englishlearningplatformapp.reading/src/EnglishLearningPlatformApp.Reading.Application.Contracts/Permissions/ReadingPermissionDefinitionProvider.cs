using EnglishLearningPlatformApp.Reading.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace EnglishLearningPlatformApp.Reading.Permissions;

public class ReadingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ReadingPermissions.GroupName, L("Permission:Reading"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ReadingResource>(name);
    }
}
