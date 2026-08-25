using XYPortal.EvGRPC.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace XYPortal.EvGRPC.Permissions;

public class EvGRPCPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(EvGRPCPermissions.GroupName, L("Permission:EvGRPC"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EvGRPCResource>(name);
    }
}
