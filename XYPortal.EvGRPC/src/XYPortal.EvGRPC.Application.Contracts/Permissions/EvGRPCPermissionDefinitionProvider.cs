using XYPortal.EvGRPC.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace XYPortal.EvGRPC.Permissions;

public class EvGRPCPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var evgrpc = context.AddGroup(EvGRPCPermissions.GroupName, L("Permission:EvGRPC"));

        var vehicle = evgrpc.AddPermission(EvGRPCPermissions.Vehicle_Default, L("Permission:Vehicle"));
        vehicle.AddChild(EvGRPCPermissions.Vehicle_Create, L("Permission:Create"));
        vehicle.AddChild(EvGRPCPermissions.Vehicle_Update, L("Permission:Update"));
        vehicle.AddChild(EvGRPCPermissions.Vehicle_Delete, L("Permission:Delete"));

        var charging = evgrpc.AddPermission(EvGRPCPermissions.Charging_Default, L("Permission:Charging"));
        charging.AddChild(EvGRPCPermissions.Charging_Create, L("Permission:Create"));
        charging.AddChild(EvGRPCPermissions.Charging_Update, L("Permission:Update"));
        charging.AddChild(EvGRPCPermissions.Charging_Delete, L("Permission:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EvGRPCResource>(name);
    }
}
