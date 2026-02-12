namespace XYPortal.Permissions;

public static class XYPortalPermissions
{
    public const string GroupName = "XYPortal";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
    public const string OpenIdDictManager = GroupName + ".OpenIdDictManager";
    public const string OpenIdDictApplicationManager = OpenIdDictManager + ".ApplicationManager";
    public const string OpenIdDictApplicationCreate = OpenIdDictApplicationManager + ".Create";
    public const string OpenIdDictApplicationEdit = OpenIdDictApplicationManager + ".Edit";
    public const string OpenIdDictApplicationDelete = OpenIdDictApplicationManager + ".Delete";
}
