using System.Threading.Tasks;
using XYPortal.LinkBoard.Permissions;
using XYPortal.Localization;
using XYPortal.MultiTenancy;
using XYPortal.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;

namespace XYPortal.Web.Menus;

public class XYPortalMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var administration = context.Menu.GetAdministration();
        var l = context.GetLocalizer<XYPortalResource>();

        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                XYPortalMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fas fa-home",
                order: 0
            )
        );

        if (MultiTenancyConsts.IsEnabled)
        {
            administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        }
        else
        {
            administration.TryRemoveMenuItem(TenantManagementMenuNames.GroupName);
        }

        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 2);
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 4);

        var openIddictMenu = new ApplicationMenuItem(
            XYPortalMenus.OpenIddictManager,
            l["Menu:OpenIdDictManager"],
            icon: "fas fa-key",
            order: 3
        ).RequirePermissions(false, XYPortalPermissions.OpenIdDictApplicationManager, XYPortalPermissions.OpenIdDictScopeManager);

        openIddictMenu.AddItem(new ApplicationMenuItem(
            XYPortalMenus.OpenIddictApplications,
            l["Menu:OpenIddictApplications"],
            "~/OpenIddict/Applications"
        ).RequirePermissions(false, XYPortalPermissions.OpenIdDictApplicationManager));

        openIddictMenu.AddItem(new ApplicationMenuItem(
            XYPortalMenus.OpenIddictScopes,
            l["Menu:OpenIddictScopes"],
            "~/OpenIddict/Scopes"
        ).RequirePermissions(false, XYPortalPermissions.OpenIdDictScopeManager));

        administration.AddItem(openIddictMenu);

        // LinkBoard User Menu
        var linkBoardMenu = new ApplicationMenuItem(
            XYPortalMenus.LinkBoard,
            l["Menu:LinkBoard"],
            icon: "fas fa-link",
            order: 1
        ).RequirePermissions(false, LinkBoardPermissions.User, LinkBoardPermissions.Admin);

        linkBoardMenu.AddItem(new ApplicationMenuItem(
            XYPortalMenus.LinkBoardCategories,
            l["Menu:LinkBoard:Categories"],
            "~/LinkBoard/Categories"
        ).RequirePermissions(false, LinkBoardPermissions.LinkCategoryManager));

        linkBoardMenu.AddItem(new ApplicationMenuItem(
            XYPortalMenus.LinkBoardLinks,
            l["Menu:LinkBoard:Links"],
            "~/LinkBoard/Links"
        ).RequirePermissions(false, LinkBoardPermissions.LinkManager));

        context.Menu.AddItem(linkBoardMenu);

        // LinkBoard Admin Menu
        var linkBoardAdminMenu = new ApplicationMenuItem(
            XYPortalMenus.LinkBoardAdmin,
            l["Menu:LinkBoardAdmin"],
            icon: "fas fa-clipboard-check",
            order: 5
        ).RequirePermissions(false, LinkBoardPermissions.Admin);

        linkBoardAdminMenu.AddItem(new ApplicationMenuItem(
            XYPortalMenus.LinkBoardCategoryReview,
            l["Menu:LinkBoardAdmin:CategoryReview"],
            "~/LinkBoard/CategoryReview"
        ).RequirePermissions(false, LinkBoardPermissions.LinkCategoryReview));

        linkBoardAdminMenu.AddItem(new ApplicationMenuItem(
            XYPortalMenus.LinkBoardLinkReview,
            l["Menu:LinkBoardAdmin:LinkReview"],
            "~/LinkBoard/LinkReview"
        ).RequirePermissions(false, LinkBoardPermissions.LinkReview));

        administration.AddItem(linkBoardAdminMenu);

        return Task.CompletedTask;
    }
}
