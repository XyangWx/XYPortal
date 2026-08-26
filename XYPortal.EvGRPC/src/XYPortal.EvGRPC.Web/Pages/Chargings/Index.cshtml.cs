using XYPortal.EvGRPC.Vehicles;
using System.Collections.Generic;
using System.Threading.Tasks;
using XYPortal.EvGRPC.Chargings;

namespace XYPortal.EvGRPC.Web.Pages.Chargings;

public class IndexModel : EvGRPCPageModel
{
    public List<ChargingDto> Chargings { get; private set; } = new();

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
    public async Task OnGetAsync(string? vehicleId = null)
    {
        if (!string.IsNullOrWhiteSpace(vehicleId))
        {
            Chargings = await _chargingService.GetListAsync(vehicleId, pageSize: 200);
            return;
        }

        var all = new List<ChargingDto>();
        var vehicles = await _vehicleService.GetListAsync(pageSize: 100);
        foreach (var v in vehicles)
        {
            var more = await _chargingService.GetListAsync(v.Id, pageSize: 200);
            all.AddRange(more);
        }
        // Show most recent first.
        all.Sort((a, b) => b.EndTime.CompareTo(a.EndTime));
        Chargings = all;
    }
}
