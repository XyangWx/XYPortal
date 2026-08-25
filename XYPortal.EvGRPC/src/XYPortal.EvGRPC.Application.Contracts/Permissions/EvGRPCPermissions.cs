using Volo.Abp.Reflection;

namespace XYPortal.EvGRPC.Permissions;

public class EvGRPCPermissions
{
    public const string GroupName = "EvGRPC";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(EvGRPCPermissions));
    }
}
