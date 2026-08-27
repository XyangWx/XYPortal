using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp;
using XYPortal.EvGRPC.Chargings;

namespace XYPortal.EvGRPC.Web.Pages.Chargings;

public class EditModalModel : EvGRPCPageModel
{
    /// <summary>
    /// Re-uses the Application.Contracts DTO so ASP.NET model
    /// binding + the existing data annotations ([Required],
    /// [Range], [StringLength]) automatically repopulate the form
    /// on validation failure.
    /// </summary>
    [BindProperty]
    public CreateUpdateChargingDto Charging { get; set; } = new();

    public List<SelectListItem> ChargerTypes { get; private set; } = new();

    private readonly IChargingAppService _service;

    public EditModalModel(IChargingAppService service)
    {
        _service = service;
    }

    public async Task OnGetAsync(string id)
    {
        var dto = await _service.GetAsync(id);
        Charging = new CreateUpdateChargingDto
        {
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

    public async Task<IActionResult> OnPostAsync(string id)
    {
        try
        {
            await _service.UpdateAsync(id, Charging);
        }
        catch (UserFriendlyException ex)
        {
            Alerts.Danger(ex.Message);
            return Page();   // re-render with user's input preserved
        }
        // Post-Redirect-Get: return to the charging list page,
        // preserving the per-vehicle filter.
        return LocalRedirect(Url.Page("/Index", new { vehicleId = Charging.VehicleId })!);
    }
}
