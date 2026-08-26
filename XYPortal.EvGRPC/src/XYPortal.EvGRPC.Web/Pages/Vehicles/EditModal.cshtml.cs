using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Web.Pages.Vehicles;

public class EditModalModel : EvGRPCPageModel
{
    [BindProperty]
    public UpdateVehicleDto Vehicle { get; set; } = new();

    private readonly IVehicleAppService _service;

    public EditModalModel(IVehicleAppService service)
    {
        _service = service;
    }

    public async Task OnGetAsync(string id)
    {
        var dto = await _service.GetAsync(id);
        Vehicle = new UpdateVehicleDto
        {
            Brand = dto.Brand,
            CalibratedRangeKm = dto.CalibratedRangeKm,
            BatteryCapacityKwh = dto.BatteryCapacityKwh,
            PurchaseDate = dto.PurchaseDate,
            LicensePlate = dto.LicensePlate,
        };
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        await _service.UpdateAsync(id, new CreateUpdateVehicleDto
        {
            Brand = Vehicle.Brand,
            CalibratedRangeKm = Vehicle.CalibratedRangeKm,
            BatteryCapacityKwh = Vehicle.BatteryCapacityKwh,
            PurchaseDate = Vehicle.PurchaseDate,
            LicensePlate = Vehicle.LicensePlate,
        });
        return NoContent();
    }

    public class UpdateVehicleDto
    {
        public string Id { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CalibratedRangeKm { get; set; }
        public double BatteryCapacityKwh { get; set; }
        public DateOnly PurchaseDate { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
    }
}
