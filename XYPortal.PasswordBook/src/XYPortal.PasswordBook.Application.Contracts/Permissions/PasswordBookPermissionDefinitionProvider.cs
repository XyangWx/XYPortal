using XYPortal.PasswordBook.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace XYPortal.PasswordBook.Permissions;

public class PasswordBookPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(PasswordBookPermissions.GroupName, L("Permission:PasswordBook"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PasswordBookResource>(name);
    }
}
