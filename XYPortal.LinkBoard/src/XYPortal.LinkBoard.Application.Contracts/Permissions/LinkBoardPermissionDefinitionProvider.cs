using XYPortal.LinkBoard.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace XYPortal.LinkBoard.Permissions;

public class LinkBoardPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(LinkBoardPermissions.GroupName, L("Permission:LinkBoard"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<LinkBoardResource>(name);
    }
}
