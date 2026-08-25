using System.Threading.Tasks;
using Volo.Abp.UI.Navigation;

namespace XYPortal.EvGRPC.Web.Menus;

public class EvGRPCMenuContributor : IMenuContributor
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
        context.Menu.AddItem(new ApplicationMenuItem(EvGRPCMenus.Prefix, displayName: "EvGRPC", "~/EvGRPC", icon: "fa fa-globe"));

        return Task.CompletedTask;
    }
}
