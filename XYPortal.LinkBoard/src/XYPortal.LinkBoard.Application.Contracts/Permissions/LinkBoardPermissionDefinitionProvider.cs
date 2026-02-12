using XYPortal.LinkBoard.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace XYPortal.LinkBoard.Permissions;

public class LinkBoardPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(LinkBoardPermissions.GroupName, L("Permission:LinkBoard"));

        // Admin
        var admin = myGroup.AddPermission(LinkBoardPermissions.Admin, L("Permission:LinkBoardAdmin"));
        admin.AddChild(LinkBoardPermissions.LinkReview, L("Permission:LinkReview"));
        admin.AddChild(LinkBoardPermissions.LinkCategoryReview, L("Permission:LinkCategoryReview"));

        // User
        var user = myGroup.AddPermission(LinkBoardPermissions.User, L("Permission:LinkBoardUser"));

        var categoryManager = user.AddChild(LinkBoardPermissions.LinkCategoryManager, L("Permission:LinkCategoryManager"));
        categoryManager.AddChild(LinkBoardPermissions.LinkCategoryCreate, L("Permission:LinkCategoryCreate"));
        categoryManager.AddChild(LinkBoardPermissions.LinkCategoryModify, L("Permission:LinkCategoryModify"));
        categoryManager.AddChild(LinkBoardPermissions.LinkCategoryDelete, L("Permission:LinkCategoryDelete"));

        var linkManager = user.AddChild(LinkBoardPermissions.LinkManager, L("Permission:LinkManager"));
        linkManager.AddChild(LinkBoardPermissions.LinkCreate, L("Permission:LinkCreate"));
        linkManager.AddChild(LinkBoardPermissions.LinkModify, L("Permission:LinkModify"));
        linkManager.AddChild(LinkBoardPermissions.LinkDelete, L("Permission:LinkDelete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<LinkBoardResource>(name);
    }
}
