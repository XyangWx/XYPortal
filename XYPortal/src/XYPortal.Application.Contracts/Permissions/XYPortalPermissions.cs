namespace XYPortal.Permissions;

// ReSharper disable once InconsistentNaming
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
    public const string OpenIdDictScopeManager = OpenIdDictManager + ".ScopeManager";
    public const string OpenIdDictScopeCreate = OpenIdDictScopeManager + ".Create";
    public const string OpenIdDictScopeEdit = OpenIdDictScopeManager + ".Edit";
    public const string OpenIdDictScopeDelete = OpenIdDictScopeManager + ".Delete";
}
