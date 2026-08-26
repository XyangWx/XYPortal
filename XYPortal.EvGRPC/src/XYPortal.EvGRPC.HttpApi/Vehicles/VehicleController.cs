using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using XYPortal.EvGRPC.Permissions;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Vehicles;

/// <summary>
/// REST surface for Vehicle CRUD. Thin controller that delegates
/// every call to <see cref="IVehicleAppService"/>. The actual
/// persistence lives in the upstream evGRpc gRPC service —
/// this module holds no local state.
/// </summary>
[Area(EvGRPCRemoteServiceConsts.ModuleName)]
[RemoteService(Name = EvGRPCRemoteServiceConsts.RemoteServiceName)]
[Route("api/ev-gRPC/vehicles")]
public class VehicleController : EvGRPCController
{
    private readonly IVehicleAppService _service;

    public VehicleController(IVehicleAppService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    [Authorize(EvGRPCPermissions.Vehicle_Default)]
    public async Task<VehicleDto> GetAsync(string id)
    {
        return await _service.GetAsync(id);
    }

    [HttpGet]
    [Authorize(EvGRPCPermissions.Vehicle_Default)]
    public async Task<List<VehicleDto>> GetListAsync(
        [FromQuery] int pageSize = 50,
        [FromQuery] string? pageToken = null)
    {
        return await _service.GetListAsync(pageSize, pageToken);
    }

    [HttpPost]
    [Authorize(EvGRPCPermissions.Vehicle_Create)]
    public async Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input)
    {
        return await _service.CreateAsync(input);
    }

    [HttpPut("{id}")]
    [Authorize(EvGRPCPermissions.Vehicle_Update)]
    public async Task<VehicleDto> UpdateAsync(string id, CreateUpdateVehicleDto input)
    {
        return await _service.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    [Authorize(EvGRPCPermissions.Vehicle_Delete)]
    public async Task DeleteAsync(string id)
    {
        await _service.DeleteAsync(id);
    }
}
