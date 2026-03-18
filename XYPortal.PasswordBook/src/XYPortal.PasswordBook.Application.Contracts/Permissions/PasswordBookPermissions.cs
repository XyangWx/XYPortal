using Volo.Abp.Reflection;

namespace XYPortal.PasswordBook.Permissions;

public class PasswordBookPermissions
{
    public const string GroupName = "PasswordBook";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(PasswordBookPermissions));
    }
}
