using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using XYPortal.EvGRPC.Chargings;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Web.Pages.Vehicles;

public class IndexModel : EvGRPCPageModel
{
    public List<VehicleDto> Vehicles { get; private set; } = new();

    /// <summary>
    /// vehicle_id -> most-recent EndPercent (0 when no charging
    /// recorded yet). Filled in one batch by walking the charging
    /// history of each vehicle.
    /// </summary>
    public Dictionary<string, int> BatteryByVehicle { get; } =
        new Dictionary<string, int>();

    private readonly IVehicleAppService _vehicleService;
    private readonly IChargingAppService _chargingService;

    public IndexModel(
        IVehicleAppService vehicleService,
        IChargingAppService chargingService)
    {
        _vehicleService = vehicleService;
        _chargingService = chargingService;
    }

    public async Task OnGetAsync()
    {
        Vehicles = await _vehicleService.GetListAsync(pageSize: 100);

        // One RPC per vehicle for the latest charging. Phase 4.4 will
        // consider a bulk-fanout if the fleet grows large.
        foreach (var v in Vehicles)
        {
            var battery = await _chargingService.GetCurrentBatteryAsync(v.Id);
            if (battery.BatteryPercent > 0)
            {
                BatteryByVehicle[v.Id] = battery.BatteryPercent;
            }
        }
    }

    /// <summary>
    /// Handler invoked by the per-row Delete form post. RpcException
    /// (e.g. upstream FK constraint) is mapped to UserFriendlyException
    /// by the AppService layer and re-surfaces here as an Abp
    /// framework alert via <see cref="HandleErrorAsync"/>.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("id is required");
        }
        try
        {
            await _vehicleService.DeleteAsync(id);
            // Refresh the list so the deleted row disappears without
            // the JS layer needing to re-navigate.
            return RedirectToPage();
        }
        catch (UserFriendlyException ex)
        {
            // Surface the upstream message in the alert stream so the
            // user sees a real error rather than a silent navigation.
            Alerts.Danger(ex.Message);
        }
        return RedirectToPage();
    }
}
