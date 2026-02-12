using Volo.Abp.Reflection;

namespace XYPortal.LinkBoard.Permissions;

public class LinkBoardPermissions
{
    public const string GroupName = "LinkBoard";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(LinkBoardPermissions));
    }
}
