using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Web.Pages.Vehicles;

public class CreateModalModel : EvGRPCPageModel
{
    [BindProperty]
    public CreateUpdateVehicleDto Vehicle { get; set; } = new();

    private readonly IVehicleAppService _service;

    public CreateModalModel(IVehicleAppService service)
    {
        _service = service;
    }

    public void OnGet()
    {
        Vehicle = new CreateUpdateVehicleDto();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = await _service.CreateAsync(Vehicle);
        return NoContent();
    }
}
