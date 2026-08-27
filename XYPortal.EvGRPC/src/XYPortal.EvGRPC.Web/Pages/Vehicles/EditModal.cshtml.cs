using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Web.Pages.Vehicles;

public class EditModalModel : EvGRPCPageModel
{
    /// <summary>
    /// Re-uses the Application.Contracts DTO so ASP.NET model
    /// binding + the existing data annotations ([Required],
    /// [StringLength], [Range]) automatically repopulate the form
    /// on validation failure. ASP.NET's tag helpers preserve
    /// user-entered values when the same page is re-rendered after
    /// a POST that fails ModelState validation.
    /// </summary>
    [BindProperty]
    public CreateUpdateVehicleDto Vehicle { get; set; } = new();

    private readonly IVehicleAppService _service;

    public EditModalModel(IVehicleAppService service)
    {
        _service = service;
    }

    public async Task OnGetAsync(string id)
    {
        var dto = await _service.GetAsync(id);
        Vehicle = new CreateUpdateVehicleDto
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
        try
        {
            await _service.UpdateAsync(id, Vehicle);
        }
        catch (UserFriendlyException ex)
        {
            // Upstream FK / not-found / domain invariant violations
            // surfaced by the AppService. Surface in the alert stream
            // and re-render the page (with the user's input preserved
            // by ASP.NET model binding).
            Alerts.Danger(ex.Message);
            return Page();
        }
        // Post-Redirect-Get: return to the list page so the user
        // sees the updated row immediately. (Form-POST → 302 → GET /
        // Vehicles → list re-renders.)
        return LocalRedirect("~/Vehicles");
    }
}
