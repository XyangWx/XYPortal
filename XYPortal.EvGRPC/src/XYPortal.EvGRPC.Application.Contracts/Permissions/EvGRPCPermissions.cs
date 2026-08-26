using Volo.Abp.Reflection;

namespace XYPortal.EvGRPC.Permissions;

public class EvGRPCPermissions
{
    public const string GroupName = "EvGRPC";

    public const string Vehicle_Default = GroupName + ".Vehicle";
    public const string Vehicle_Create = Vehicle_Default + ".Create";
    public const string Vehicle_Update = Vehicle_Default + ".Update";
    public const string Vehicle_Delete = Vehicle_Default + ".Delete";

    public const string Charging_Default = GroupName + ".Charging";
    public const string Charging_Create = Charging_Default + ".Create";
    public const string Charging_Update = Charging_Default + ".Update";
    public const string Charging_Delete = Charging_Default + ".Delete";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(EvGRPCPermissions));
    }
}
