using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;

namespace XYPortal.RandomStringProvider.Web.Menus;

public class RandomStringProviderMenuContributor : IMenuContributor
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
        context.Menu.AddItem(new ApplicationMenuItem(RandomStringProviderMenus.Prefix, displayName: "RandomStringProvider", "~/RandomStringProvider", icon: "fa fa-globe"));

        return Task.CompletedTask;
    }
}
