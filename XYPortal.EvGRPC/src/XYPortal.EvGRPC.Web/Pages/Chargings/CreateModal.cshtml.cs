using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XYPortal.EvGRPC.Chargings;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Web.Pages.Chargings;

public class CreateModalModel : EvGRPCPageModel
{
    [BindProperty]
    public CreateUpdateChargingDto Charging { get; set; } = new();

    public List<SelectListItem> Vehicles { get; private set; } = new();
    public List<SelectListItem> ChargerTypes { get; private set; } = new();

    private readonly IChargingAppService _chargingService;
    private readonly IVehicleAppService _vehicleService;

    public CreateModalModel(IChargingAppService chargingService, IVehicleAppService vehicleService)
    {
        _chargingService = chargingService;
        _vehicleService = vehicleService;
    }

    public async Task OnGetAsync(string? vehicleId = null)
    {
        Charging = new CreateUpdateChargingDto
        {
            VehicleId = vehicleId ?? string.Empty,
            StartTime = DateTimeOffset.Now.AddHours(-1),
            EndTime = DateTimeOffset.Now,
            ChargerType = ChargerType.Fast,
        };
        await PopulateLookupsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _chargingService.CreateAsync(Charging);
        return NoContent();
    }

    private async Task PopulateLookupsAsync()
    {
        var vehicles = await _vehicleService.GetListAsync(pageSize: 200);
        Vehicles = vehicles.Select(v => new SelectListItem(
            $"{v.Brand} · {v.LicensePlate}",
            v.Id,
            v.Id == Charging.VehicleId)).ToList();

        ChargerTypes = Enum.GetValues<ChargerType>()
            .Select(ct => new SelectListItem(ct.ToString(), ct.ToString(),
                ct == Charging.ChargerType))
            .ToList();
    }
}
