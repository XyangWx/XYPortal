using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.UI.Navigation;
using XYPortal.PasswordBook.Permissions;

namespace XYPortal.PasswordBook.Web.Menus;

public class PasswordBookMenuContributor : IMenuContributor
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
        // Add main menu items
        context.Menu.AddItem(new ApplicationMenuItem(
            PasswordBookMenus.Prefix,
            "PasswordBook",
            "~/PasswordBook",
            icon: "fa fa-lock",
            requiredPermissionName: PasswordBookPermissions.PassWordBookUser
        ));

        return Task.CompletedTask;
    }
}

public class PasswordBookMenus
{
    public const string Prefix = "PasswordBook";
}
