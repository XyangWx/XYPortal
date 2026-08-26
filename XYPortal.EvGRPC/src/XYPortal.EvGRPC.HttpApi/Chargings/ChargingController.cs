using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using XYPortal.EvGRPC.Chargings;
using XYPortal.EvGRPC.Permissions;

namespace XYPortal.EvGRPC.Chargings;

[Area(EvGRPCRemoteServiceConsts.ModuleName)]
[RemoteService(Name = EvGRPCRemoteServiceConsts.RemoteServiceName)]
[Route("api/ev-gRPC/chargings")]
public class ChargingController : EvGRPCController
{
    private readonly IChargingAppService _service;

    public ChargingController(IChargingAppService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    [Authorize(EvGRPCPermissions.Charging_Default)]
    public async Task<ChargingDto> GetAsync(string id)
    {
        return await _service.GetAsync(id);
    }

    [HttpGet]
    [Authorize(EvGRPCPermissions.Charging_Default)]
    public async Task<List<ChargingDto>> GetListAsync(
        [FromQuery] string vehicleId,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? pageToken = null)
    {
        return await _service.GetListAsync(vehicleId, pageSize, pageToken);
    }

    [HttpPost]
    [Authorize(EvGRPCPermissions.Charging_Create)]
    public async Task<ChargingDto> CreateAsync(CreateUpdateChargingDto input)
    {
        return await _service.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(EvGRPCPermissions.Charging_Update)]
    public async Task<ChargingDto> UpdateAsync(string id, CreateUpdateChargingDto input)
    {
        return await _service.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(EvGRPCPermissions.Charging_Delete)]
    public async Task DeleteAsync(string id)
    {
        await _service.DeleteAsync(id);
    }
}
