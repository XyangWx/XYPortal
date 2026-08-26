using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using XYPortal.EvGRPC.Chargings;
using XYPortal.EvGRPC.Permissions;

namespace XYPortal.EvGRPC.Chargings;

[Area(EvGRPCRemoteServiceConsts.ModuleName)]
[RemoteService(Name = EvGRPCRemoteServiceConsts.RemoteServiceName)]
[Route("api/ev-gRPC/vehicles/{vehicleId}/battery")]
public class CurrentBatteryController : EvGRPCController
{
    private readonly IChargingAppService _service;

    public CurrentBatteryController(IChargingAppService service)
    {
        _service = service;
    }

    /// <summary>
    /// Returns the most-recent battery percent for the given
    /// vehicle. Returns 0% with a null timestamp when no charging
    /// has been recorded yet.
    /// </summary>
    [HttpGet]
    [Authorize(EvGRPCPermissions.Vehicle_Default)]
    public async Task<CurrentBatteryDto> GetAsync(string vehicleId)
    {
        return await _service.GetCurrentBatteryAsync(vehicleId);
    }
}
