using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using XYPortal.EvGRPC.EvGrpc;
using XYPortal.EvGRPC.Vehicles;

namespace XYPortal.EvGRPC.Vehicles;

/// <summary>
/// Application service that orchestrates Vehicle CRUD through
/// the upstream evGRpc gRPC service. Holds no DB state itself —
/// every method round-trips to evGRpc, so all the persistence
/// lives behind that boundary (constraint C-2 in the brainstorm).
///
/// gRPC errors are translated to ABP <see cref="UserFriendlyException"/>
/// so the UI / API consumer sees a readable message instead of a
/// raw <c>RpcException</c> (AC-4 from the brainstorm).
/// </summary>
public class VehicleAppService : EvGRPCAppService, IVehicleAppService
{
    private readonly EvGrpcClient _client;
    private readonly VehicleMappers _vehicleMappers;
    private readonly ILogger<VehicleAppService> _logger;

    public VehicleAppService(
        EvGrpcClient client,
        VehicleMappers vehicleMappers,
        ILogger<VehicleAppService> logger)
    {
        _client = client;
        _vehicleMappers = vehicleMappers;
        _logger = logger;
    }

    public async Task<VehicleDto> GetAsync(string id)
    {
        try
        {
            var entity = await _client.GetVehicleAsync(id);
            return _vehicleMappers.Map(entity);
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(GetAsync), id);
        }
    }

    public async Task<List<VehicleDto>> GetListAsync(int pageSize = 50, string? pageToken = null)
    {
        try
        {
            var entities = await _client.ListVehiclesAsync(pageSize, pageToken);
            return entities.Select(_vehicleMappers.Map).ToList();
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(GetListAsync), pageToken);
        }
    }

    public async Task<VehicleDto> CreateAsync(CreateUpdateVehicleDto input)
    {
        try
        {
            var entity = Vehicle.Create(
                brand: input.Brand,
                calibratedRangeKm: input.CalibratedRangeKm,
                batteryCapacityKwh: input.BatteryCapacityKwh,
                purchaseDate: input.PurchaseDate,
                licensePlate: input.LicensePlate);
            var created = await _client.CreateVehicleAsync(entity);
            return _vehicleMappers.Map(created);
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(CreateAsync), input.LicensePlate);
        }
    }

    public async Task<VehicleDto> UpdateAsync(string id, CreateUpdateVehicleDto input)
    {
        try
        {
            var entity = new Vehicle(
                id: id,
                brand: input.Brand,
                calibratedRangeKm: input.CalibratedRangeKm,
                batteryCapacityKwh: input.BatteryCapacityKwh,
                purchaseDate: input.PurchaseDate,
                licensePlate: input.LicensePlate);
            var updated = await _client.UpdateVehicleAsync(entity);
            return _vehicleMappers.Map(updated);
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(UpdateAsync), id);
        }
    }

    public async Task DeleteAsync(string id)
    {
        try
        {
            await _client.DeleteVehicleAsync(id);
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(DeleteAsync), id);
        }
    }

    private UserFriendlyException TranslateRpcException(RpcException ex, string op, string? key) =>
        new(
            $"evGRpc call {op} failed (key='{key ?? "<none>"}'): {ex.Status.StatusCode} {ex.Status.Detail}",
            "EvGRPC:UpstreamError")
        {
            // Preserve the upstream status as data so callers can branch on it.
            Data = { ["UpstreamStatus"] = ex.Status.StatusCode.ToString() }
        };
}
