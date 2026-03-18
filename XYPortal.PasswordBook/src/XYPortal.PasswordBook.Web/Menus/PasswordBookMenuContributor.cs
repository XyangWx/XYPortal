using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;

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
        //Add main menu items.
        context.Menu.AddItem(new ApplicationMenuItem(PasswordBookMenus.Prefix, displayName: "PasswordBook", "~/PasswordBook", icon: "fa fa-globe"));

        return Task.CompletedTask;
    }
}
