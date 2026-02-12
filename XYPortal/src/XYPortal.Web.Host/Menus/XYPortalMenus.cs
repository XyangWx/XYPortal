namespace XYPortal.Web.Menus;

public class XYPortalMenus
{
    private const string Prefix = "XYPortal";
    public const string Home = Prefix + ".Home";

    public const string OpenIddictManager = Prefix + ".OpenIddictManager";
    public const string OpenIddictApplications = OpenIddictManager + ".Applications";
    public const string OpenIddictScopes = OpenIddictManager + ".Scopes";

    // LinkBoard User
    public const string LinkBoard = Prefix + ".LinkBoard";
    public const string LinkBoardCategories = LinkBoard + ".Categories";
    public const string LinkBoardLinks = LinkBoard + ".Links";

    // LinkBoard Admin
    public const string LinkBoardAdmin = Prefix + ".LinkBoardAdmin";
    public const string LinkBoardCategoryReview = LinkBoardAdmin + ".CategoryReview";
    public const string LinkBoardLinkReview = LinkBoardAdmin + ".LinkReview";
}
