using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Evgrpc;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using XYPortal.EvGRPC.Chargings;
using XYPortal.EvGRPC.EvGrpc.Mapping;
// Alias to disambiguate from Evgrpc.{Vehicle,Charging} proto classes.
using DomainVehicle = XYPortal.EvGRPC.Vehicles.Vehicle;
using DomainCharging = XYPortal.EvGRPC.Chargings.Charging;

namespace XYPortal.EvGRPC.EvGrpc;

/// <summary>
/// Typed client for the upstream evGRpc service. Wraps one
/// long-lived <see cref="GrpcChannel"/> and exposes 8 CRUD methods
/// across <see cref="Vehicle"/> and <see cref="Charging"/> that
/// convert proto messages to Domain entities at the boundary.
///
/// The class is registered as a singleton (one channel per process)
/// because <c>GrpcChannel</c> is designed for long-lived reuse — a
/// new channel per RPC would defeat the multiplexed HTTP/2 keep-alive.
///
/// Phase 3.5 will add JWT metadata injection from the current user;
/// for now <c>EvGrpc:AccessToken</c> (static, from configuration)
/// is the only auth path.
/// </summary>
public sealed class EvGrpcClient : IDisposable
{
    private readonly EvGrpcOptions _options;
    private readonly ILogger<EvGrpcClient> _logger;
    private readonly Lazy<GrpcChannel> _channel;
    private readonly Lazy<VehicleService.VehicleServiceClient> _vehicles;
    private readonly Lazy<ChargingService.ChargingServiceClient> _chargings;
    private bool _disposed;

    public EvGrpcClient(
        IOptions<EvGrpcOptions> options,
        ILogger<EvGrpcClient>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
        _logger = logger ?? NullLogger<EvGrpcClient>.Instance;

        if (string.IsNullOrWhiteSpace(_options.Url))
        {
            throw new InvalidOperationException(
                "EvGrpc:Url is not configured. Set it in appsettings.json " +
                "under the EvGrpc section before resolving EvGrpcClient.");
        }

        _channel = new Lazy<GrpcChannel>(BuildChannel, isThreadSafe: true);
        _vehicles = new Lazy<VehicleService.VehicleServiceClient>(
            () => new VehicleService.VehicleServiceClient(_channel.Value),
            isThreadSafe: true);
        _chargings = new Lazy<ChargingService.ChargingServiceClient>(
            () => new ChargingService.ChargingServiceClient(_channel.Value),
            isThreadSafe: true);
    }

    /// <summary>
    /// Eagerly initialize the underlying gRPC channel. Idempotent
    /// and cheap on subsequent calls. Kept from Phase 1 as an
    /// explicit wire-smoke-test hook (no RPC, just channel setup).
    /// </summary>
    public bool Connect()
    {
        _ = _channel.Value;
        _logger.LogInformation("EvGRPC channel initialized for {Url}", _options.Url);
        return true;
    }

    // ---------- Vehicle ----------

    public async Task<DomainVehicle> CreateVehicleAsync(DomainVehicle vehicle, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        var request = vehicle.ToCreateRequest();
        var response = await _vehicles.Value.CreateVehicleAsync(request, cancellationToken: ct).ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task<DomainVehicle> GetVehicleAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id must be non-blank.", nameof(id));
        var response = await _vehicles.Value
            .GetVehicleAsync(new GetVehicleRequest { Id = id }, cancellationToken: ct)
            .ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task<DomainVehicle> UpdateVehicleAsync(DomainVehicle vehicle, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        var request = vehicle.ToUpdateRequest();
        var response = await _vehicles.Value.UpdateVehicleAsync(request, cancellationToken: ct).ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task DeleteVehicleAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id must be non-blank.", nameof(id));
        await _vehicles.Value
            .DeleteVehicleAsync(new DeleteVehicleRequest { Id = id }, cancellationToken: ct)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DomainVehicle>> ListVehiclesAsync(int pageSize, string? pageToken, CancellationToken ct = default)
    {
        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        var response = await _vehicles.Value
            .ListVehiclesAsync(
                new ListVehiclesRequest
                {
                    PageSize = pageSize,
                    PageToken = pageToken ?? string.Empty,
                },
                cancellationToken: ct)
            .ConfigureAwait(false);
        var list = new List<DomainVehicle>(response.Vehicles.Count);
        foreach (var v in response.Vehicles) list.Add(v.ToDomain());
        return list;
    }

    // ---------- Charging ----------

    public async Task<DomainCharging> CreateChargingAsync(DomainCharging charging, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(charging);
        var request = charging.ToCreateRequest();
        var response = await _chargings.Value.CreateChargingAsync(request, cancellationToken: ct).ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task<DomainCharging> GetChargingAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id must be non-blank.", nameof(id));
        var response = await _chargings.Value
            .GetChargingAsync(new GetChargingRequest { Id = id }, cancellationToken: ct)
            .ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task<DomainCharging> UpdateChargingAsync(DomainCharging charging, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(charging);
        var request = charging.ToUpdateRequest();
        var response = await _chargings.Value.UpdateChargingAsync(request, cancellationToken: ct).ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task DeleteChargingAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id must be non-blank.", nameof(id));
        await _chargings.Value
            .DeleteChargingAsync(new DeleteChargingRequest { Id = id }, cancellationToken: ct)
            .ConfigureAwait(false);
    }

    // ---------- Channel plumbing ----------

    private GrpcChannel BuildChannel()
    {
        var credentials = string.IsNullOrWhiteSpace(_options.AccessToken)
            ? ChannelCredentials.Insecure
            : ChannelCredentials.Create(
                new SslCredentials(),
                CallCredentials.FromInterceptor((_, metadata) =>
                {
                    metadata.Add("authorization", $"Bearer {_options.AccessToken}");
                    return Task.CompletedTask;
                }));

        return GrpcChannel.ForAddress(_options.Url, new GrpcChannelOptions
        {
            Credentials = credentials,
            // 16 MiB matches proto defaults; explicit so future tuning has a hook.
            MaxReceiveMessageSize = 16 * 1024 * 1024,
            MaxSendMessageSize = 16 * 1024 * 1024,
        });
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_channel.IsValueCreated)
        {
            _channel.Value.Dispose();
        }
    }
}
