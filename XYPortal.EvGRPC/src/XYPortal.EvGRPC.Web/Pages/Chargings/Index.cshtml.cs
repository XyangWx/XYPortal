using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using XYPortal.EvGRPC.Chargings;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Web.Pages.Chargings;

public class IndexModel : EvGRPCPageModel
{
    public List<ChargingDto> Chargings { get; private set; } = new();

    /// <summary>
    /// Captured from the route so the Delete redirect can preserve
    /// the per-vehicle filter on the list page.
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public string? VehicleId { get; set; }

    private readonly IChargingAppService _chargingService;
    private readonly IVehicleAppService _vehicleService;

    public IndexModel(IChargingAppService chargingService, IVehicleAppService vehicleService)
    {
        _chargingService = chargingService;
        _vehicleService = vehicleService;
    }

    /// <summary>
    /// List chargings. If <paramref name="vehicleId"/> is supplied,
    /// list only that vehicle's charges; otherwise list across all
    /// vehicles by walking every page.
    /// </summary>
    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(VehicleId))
        {
            Chargings = await _chargingService.GetListAsync(VehicleId, pageSize: 200);
            return;
        }

        var all = new List<ChargingDto>();
        var vehicles = await _vehicleService.GetListAsync(pageSize: 100);
        foreach (var v in vehicles)
        {
            var more = await _chargingService.GetListAsync(v.Id, pageSize: 200);
            all.AddRange(more);
        }
        all.Sort((a, b) => b.EndTime.CompareTo(a.EndTime));
        Chargings = all;
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("id is required");
        }
        try
        {
            await _chargingService.DeleteAsync(id);
            // Preserve the per-vehicle filter (or the empty=all view).
            return RedirectToPage(new { vehicleId = VehicleId });
        }
        catch (UserFriendlyException ex)
        {
            Alerts.Danger(ex.Message);
            return RedirectToPage(new { vehicleId = VehicleId });
        }
    }
}
