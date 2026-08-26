using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XYPortal.EvGRPC.Chargings;

namespace XYPortal.EvGRPC.Web.Pages.Chargings;

public class EditModalModel : EvGRPCPageModel
{
    [BindProperty]
    public UpdateChargingDto Charging { get; set; } = new();

    public List<SelectListItem> ChargerTypes { get; private set; } = new();

    private readonly IChargingAppService _service;

    public EditModalModel(IChargingAppService service)
    {
        _service = service;
    }

    public async Task OnGetAsync(string id)
    {
        var dto = await _service.GetAsync(id);
        Charging = new UpdateChargingDto
        {
            Id = dto.Id,
            VehicleId = dto.VehicleId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            StartPercent = dto.StartPercent,
            EndPercent = dto.EndPercent,
            StartMileageKm = dto.StartMileageKm,
            EndMileageKm = dto.EndMileageKm,
            KwhCharged = dto.KwhCharged,
            Cost = dto.Cost,
            ElectricityUnitPrice = dto.ElectricityUnitPrice,
            ServiceFee = dto.ServiceFee,
            ChargerType = dto.ChargerType,
            SourceCategoryId = dto.SourceCategoryId,
            Location = dto.Location,
            Remark = dto.Remark,
        };

        ChargerTypes = Enum.GetValues<ChargerType>()
            .Select(ct => new SelectListItem(ct.ToString(), ct.ToString(),
                ct == dto.ChargerType))
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateAsync(Charging.Id, new CreateUpdateChargingDto
        {
            VehicleId = Charging.VehicleId,
            StartTime = Charging.StartTime,
            EndTime = Charging.EndTime,
            StartPercent = Charging.StartPercent,
            EndPercent = Charging.EndPercent,
            StartMileageKm = Charging.StartMileageKm,
            EndMileageKm = Charging.EndMileageKm,
            KwhCharged = Charging.KwhCharged,
            Cost = Charging.Cost,
            ElectricityUnitPrice = Charging.ElectricityUnitPrice,
            ServiceFee = Charging.ServiceFee,
            ChargerType = Charging.ChargerType,
            SourceCategoryId = Charging.SourceCategoryId,
            Location = Charging.Location,
            Remark = Charging.Remark,
        });
        return NoContent();
    }

    public class UpdateChargingDto
    {
        public string Id { get; set; } = string.Empty;
        public string VehicleId { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int StartPercent { get; set; }
        public int EndPercent { get; set; }
        public int StartMileageKm { get; set; }
        public int EndMileageKm { get; set; }
        public double KwhCharged { get; set; }
        public double Cost { get; set; }
        public double ElectricityUnitPrice { get; set; }
        public double? ServiceFee { get; set; }
        public ChargerType ChargerType { get; set; }
        public string SourceCategoryId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? Remark { get; set; }
    }
}
