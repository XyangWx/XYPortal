using XYPortal.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace XYPortal.Permissions;

public class XYPortalPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(XYPortalPermissions.GroupName, L("Permission:PortalManagement"));

        var openIddictManager = myGroup.AddPermission(
            XYPortalPermissions.OpenIdDictManager,
            L("Permission:OpenIdDictManager"));

        var applicationManager = openIddictManager.AddChild(
            XYPortalPermissions.OpenIdDictApplicationManager,
            L("Permission:OpenIdDictApplicationManager"));

        applicationManager.AddChild(
            XYPortalPermissions.OpenIdDictApplicationCreate,
            L("Permission:OpenIdDictApplicationCreate"));

        applicationManager.AddChild(
            XYPortalPermissions.OpenIdDictApplicationEdit,
            L("Permission:OpenIdDictApplicationEdit"));

        applicationManager.AddChild(
            XYPortalPermissions.OpenIdDictApplicationDelete,
            L("Permission:OpenIdDictApplicationDelete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<XYPortalResource>(name);
    }
}
