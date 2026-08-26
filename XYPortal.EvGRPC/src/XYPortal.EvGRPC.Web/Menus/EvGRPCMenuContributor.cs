using System.Linq;
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
        if (context.Menu.Items.All(i => i.Name != EvGRPCMenus.Prefix))
        {
            context.Menu.AddItem(new ApplicationMenuItem(
                EvGRPCMenus.Prefix,
                displayName: "EvGRPC",
                "~/EvGRPC",
                icon: "fa fa-globe"));
        }

        var evgrpc = context.Menu.Items.First(i => i.Name == EvGRPCMenus.Prefix);

        if (evgrpc.Items.All(i => i.Name != EvGRPCMenus.Vehicles))
        {
            evgrpc.AddItem(new ApplicationMenuItem(
                EvGRPCMenus.Vehicles,
                displayName: "Vehicles",
                "~/Vehicles",
                icon: "fa fa-car"));
        }

        if (evgrpc.Items.All(i => i.Name != EvGRPCMenus.Chargings))
        {
            evgrpc.AddItem(new ApplicationMenuItem(
                EvGRPCMenus.Chargings,
                displayName: "Chargings",
                "~/Chargings",
                icon: "fa fa-bolt"));
        }

        return Task.CompletedTask;
    }
}
