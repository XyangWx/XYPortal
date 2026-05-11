using Volo.Abp.Reflection;

namespace XYPortal.RandomStringProvider.Permissions;

public class RandomStringProviderPermissions
{
    public const string GroupName = "RandomStringProvider";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(RandomStringProviderPermissions));
    }
}
