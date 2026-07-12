using EnglishLearningPlatformApp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace EnglishLearningPlatformApp.Permissions;

public class EnglishLearningPlatformAppPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(EnglishLearningPlatformAppPermissions.GroupName);

        var booksPermission = myGroup.AddPermission(EnglishLearningPlatformAppPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(EnglishLearningPlatformAppPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(EnglishLearningPlatformAppPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(EnglishLearningPlatformAppPermissions.Books.Delete, L("Permission:Books.Delete"));

        var authorsPermission = myGroup.AddPermission(EnglishLearningPlatformAppPermissions.Authors.Default, L("Permission:Authors"));
        authorsPermission.AddChild(EnglishLearningPlatformAppPermissions.Authors.Create, L("Permission:Authors.Create"));
        authorsPermission.AddChild(EnglishLearningPlatformAppPermissions.Authors.Edit, L("Permission:Authors.Edit"));
        authorsPermission.AddChild(EnglishLearningPlatformAppPermissions.Authors.Delete, L("Permission:Authors.Delete"));
        var contentPermission = myGroup.AddPermission(
            EnglishLearningPlatformAppPermissions.Content.Default,
            L("Permission:Content"),
            multiTenancySide: MultiTenancySides.Tenant);
        contentPermission.AddChild(EnglishLearningPlatformAppPermissions.Content.Create, L("Permission:Content.Create"));
        contentPermission.AddChild(EnglishLearningPlatformAppPermissions.Content.Edit, L("Permission:Content.Edit"));
        contentPermission.AddChild(EnglishLearningPlatformAppPermissions.Content.Publish, L("Permission:Content.Publish"));
        contentPermission.AddChild(EnglishLearningPlatformAppPermissions.Content.Archive, L("Permission:Content.Archive"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(EnglishLearningPlatformAppPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EnglishLearningPlatformAppResource>(name);
    }
}
