namespace EnglishLearningPlatformApp.Permissions;

public static class EnglishLearningPlatformAppPermissions
{
    public const string GroupName = "EnglishLearningPlatformApp";


    public static class Books
    {
        public const string Default = GroupName + ".Books";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Authors
    {
        public const string Default = GroupName + ".Authors";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class Content
    {
        public const string Default = GroupName + ".Content";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Publish = Default + ".Publish";
        public const string Archive = Default + ".Archive";
    }
    
    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}
