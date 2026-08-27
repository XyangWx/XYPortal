using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using XYPortal.EvGRPC.Application.EvGrpc;
using DomainCharging = XYPortal.EvGRPC.Chargings.Charging;

namespace XYPortal.EvGRPC.Chargings;

public class ChargingAppService : EvGRPCAppService, IChargingAppService
{
    private readonly IEvGrpcClient _client;
    private readonly ChargingMappers _mappers;
    private readonly ILogger<ChargingAppService> _logger;

    public ChargingAppService(
        IEvGrpcClient client,
        ChargingMappers mappers,
        ILogger<ChargingAppService> logger)
    {
        _client = client;
        _mappers = mappers;
        _logger = logger;
    }

    public async Task<ChargingDto> GetAsync(string id)
    {
        try
        {
            var entity = await _client.GetChargingAsync(id);
            return _mappers.Map(entity);
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(GetAsync), id);
        }
    }

    public async Task<List<ChargingDto>> GetListAsync(string vehicleId, int pageSize = 50, string? pageToken = null)
    {
        try
        {
            var entities = await _client.ListChargingsAsync(vehicleId, pageSize: pageSize, pageToken: pageToken);
            return entities.Select(_mappers.Map).ToList();
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(GetListAsync), vehicleId);
        }
    }

    public async Task<ChargingDto> CreateAsync(CreateUpdateChargingDto input)
    {
        try
        {
            var entity = DomainCharging.Create(
                vehicleId: input.VehicleId,
                startTime: input.StartTime,
                endTime: input.EndTime,
                startPercent: input.StartPercent,
                endPercent: input.EndPercent,
                startMileageKm: input.StartMileageKm,
                endMileageKm: input.EndMileageKm,
                kwhCharged: input.KwhCharged,
                cost: input.Cost,
                electricityUnitPrice: input.ElectricityUnitPrice,
                serviceFee: input.ServiceFee,
                chargerType: input.ChargerType,
                sourceCategoryId: input.SourceCategoryId,
                location: input.Location,
                remark: input.Remark);
            var created = await _client.CreateChargingAsync(entity);
            return _mappers.Map(created);
        }
        catch (ArgumentException ex)
        {
            // Domain invariant violation (e.g. blank vehicleId, end
            // percent below start percent, end time before start time).
            throw new UserFriendlyException(ex.Message, "EvGRPC:ValidationError");
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(CreateAsync), input.VehicleId);
        }
    }

    public async Task<ChargingDto> UpdateAsync(string id, CreateUpdateChargingDto input)
    {
        try
        {
            var entity = new DomainCharging(
                id: id,
                vehicleId: input.VehicleId,
                startTime: input.StartTime,
                endTime: input.EndTime,
                startPercent: input.StartPercent,
                endPercent: input.EndPercent,
                startMileageKm: input.StartMileageKm,
                endMileageKm: input.EndMileageKm,
                kwhCharged: input.KwhCharged,
                cost: input.Cost,
                electricityUnitPrice: input.ElectricityUnitPrice,
                serviceFee: input.ServiceFee,
                chargerType: input.ChargerType,
                sourceCategoryId: input.SourceCategoryId,
                location: input.Location,
                remark: input.Remark);
            var updated = await _client.UpdateChargingAsync(entity);
            return _mappers.Map(updated);
        }
        catch (ArgumentException ex)
        {
            throw new UserFriendlyException(ex.Message, "EvGRPC:ValidationError");
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
            await _client.DeleteChargingAsync(id);
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(DeleteAsync), id);
        }
    }

    public async Task<CurrentBatteryDto> GetCurrentBatteryAsync(string vehicleId)
    {
        try
        {
            var latest = await _client.GetLatestChargingAsync(vehicleId);
            if (latest is null)
            {
                return new CurrentBatteryDto { BatteryPercent = 0, LastChargingEndTime = null };
            }
            return new CurrentBatteryDto
            {
                BatteryPercent = latest.EndPercent,
                LastChargingEndTime = latest.EndTime,
            };
        }
        catch (RpcException ex)
        {
            throw TranslateRpcException(ex, nameof(GetCurrentBatteryAsync), vehicleId);
        }
    }

    private UserFriendlyException TranslateRpcException(RpcException ex, string op, string? key) =>
        new(
            $"evGRpc call {op} failed (key='{key ?? "<none>"}'): {ex.Status.StatusCode} {ex.Status.Detail}",
            "EvGRPC:UpstreamError")
        {
            Data = { ["UpstreamStatus"] = ex.Status.StatusCode.ToString() }
        };
}
