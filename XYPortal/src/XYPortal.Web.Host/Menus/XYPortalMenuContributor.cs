using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Volo.Abp.Account.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;
using Volo.Abp.UI.Navigation;
using XYPortal.LinkBoard.Permissions;
using XYPortal.Localization;
using XYPortal.MultiTenancy;
using XYPortal.Permissions;

namespace XYPortal.Web.Menus;

public class XYPortalMenuContributor : IMenuContributor
{
    private readonly IConfiguration _configuration;

    public XYPortalMenuContributor(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
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
        ).RequirePermissions(XYPortalPermissions.OpenIdDictApplicationManager));

        openIddictMenu.AddItem(new ApplicationMenuItem(
            XYPortalMenus.OpenIddictScopes,
            l["Menu:OpenIddictScopes"],
            "~/OpenIddict/Scopes"
        ).RequirePermissions(XYPortalPermissions.OpenIdDictScopeManager));

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

    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<XYPortalResource>();
        var accountStringLocalizer = context.GetLocalizer<AccountResource>();
        var authServerUrl = _configuration["AuthServer:Authority"] ?? "";

        context.Menu.AddItem(new ApplicationMenuItem("Account.Manage", accountStringLocalizer["MyAccount"],
            $"{authServerUrl.EnsureEndsWith('/')}Account/Manage?returnUrl={_configuration["App:SelfUrl"]}", icon: "fa fa-cog", order: 1000, null, "_blank").RequireAuthenticated());
        context.Menu.AddItem(new ApplicationMenuItem("Account.Logout", l["Logout"], url: "~/Account/Logout", icon: "fa fa-power-off", order: int.MaxValue - 1000).RequireAuthenticated());

        return Task.CompletedTask;
    }
}
