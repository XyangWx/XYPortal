using Volo.Abp.Reflection;

namespace XYPortal.LinkBoard.Permissions;

public class LinkBoardPermissions
{
    public const string GroupName = "LinkBoard";

    // Admin
    public const string Admin = GroupName + ".Admin";
    public const string LinkReview = Admin + ".LinkReview";
    public const string LinkCategoryReview = Admin + ".LinkCategoryReview";

    // User
    public const string User = GroupName + ".User";
    public const string LinkCategoryManager = User + ".LinkCategoryManager";
    public const string LinkCategoryCreate = LinkCategoryManager + ".Create";
    public const string LinkCategoryModify = LinkCategoryManager + ".Modify";
    public const string LinkCategoryDelete = LinkCategoryManager + ".Delete";
    public const string LinkCategorySubmit = LinkCategoryManager + ".Submit";
    public const string LinkManager = User + ".LinkManager";
    public const string LinkCreate = LinkManager + ".Create";
    public const string LinkModify = LinkManager + ".Modify";
    public const string LinkDelete = LinkManager + ".Delete";
    public const string LinkSubmit = LinkManager + ".Submit";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(LinkBoardPermissions));
    }
}
