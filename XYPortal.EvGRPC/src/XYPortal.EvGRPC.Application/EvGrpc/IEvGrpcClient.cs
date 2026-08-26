using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XYPortal.EvGRPC.Chargings;
using XYPortal.EvGRPC.Vehicles;
using DomainVehicle = XYPortal.EvGRPC.Vehicles.Vehicle;
using DomainCharging = XYPortal.EvGRPC.Chargings.Charging;

namespace XYPortal.EvGRPC.Application.EvGrpc;

/// <summary>
/// Service-layer-facing handle over the upstream evGRpc gRPC client.
/// AppServices depend on this interface, not on the concrete
/// <see cref="global::XYPortal.EvGRPC.EvGrpc.EvGrpcClient"/>, so the
/// per-call token injection (Decorator) is testable and replaceable.
///
/// Method signatures mirror EvGrpcClient 1:1; the decorator forwards
/// every call after attaching the current user's bearer token to the
/// outgoing metadata.
/// </summary>
public interface IEvGrpcClient
{
    Task<DomainVehicle> CreateVehicleAsync(DomainVehicle vehicle, CancellationToken ct = default);
    Task<DomainVehicle> GetVehicleAsync(string id, CancellationToken ct = default);
    Task<DomainVehicle> UpdateVehicleAsync(DomainVehicle vehicle, CancellationToken ct = default);
    Task DeleteVehicleAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<DomainVehicle>> ListVehiclesAsync(int pageSize, string? pageToken, CancellationToken ct = default);

    Task<DomainCharging> CreateChargingAsync(DomainCharging charging, CancellationToken ct = default);
    Task<DomainCharging> GetChargingAsync(string id, CancellationToken ct = default);
    Task<DomainCharging> UpdateChargingAsync(DomainCharging charging, CancellationToken ct = default);
    Task DeleteChargingAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<DomainCharging>> ListChargingsAsync(
        string vehicleId,
        DateTimeOffset? startAfter = null,
        DateTimeOffset? startBefore = null,
        ChargerType? chargerType = null,
        string? sourceCategoryId = null,
        int pageSize = 100,
        string? pageToken = null,
        CancellationToken ct = default);
    Task<DomainCharging?> GetLatestChargingAsync(string vehicleId, CancellationToken ct = default);
}
