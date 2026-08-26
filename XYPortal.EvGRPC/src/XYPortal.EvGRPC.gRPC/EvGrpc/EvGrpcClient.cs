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
/// <para>
/// <b>Per-call auth:</b> the channel carries an injected
/// <see cref="MetadataFactory"/> hook. Every RPC asks the factory
/// for fresh gRPC metadata (typically a bearer token) right before
/// dispatch. The hook is async-aware, so it can resolve the current
/// user's token from the active DI scope (e.g.
/// <c>ICurrentPrincipalAccessor</c>). The default factory is a
/// no-op; production wires it via <c>EvGrpcClientDecorator</c> in
/// the Application layer.
/// </para>
/// </summary>
public sealed class EvGrpcClient : IDisposable
{
    private readonly EvGrpcOptions _options;
    private readonly ILogger<EvGrpcClient> _logger;
    private readonly Lazy<GrpcChannel> _channel;
    private readonly Lazy<VehicleService.VehicleServiceClient> _vehicles;
    private readonly Lazy<ChargingService.ChargingServiceClient> _chargings;
    private bool _disposed;

    /// <summary>
    /// Hook for per-RPC metadata injection. Default is no-op
    /// (empty Metadata); the decorator replaces this at decoration
    /// time so that every RPC carries the current user's bearer
    /// token, not whatever the channel ctor saw at startup.
    /// </summary>
    public Func<CancellationToken, Task<Metadata>> MetadataFactory { get; set; }
        = _ => Task.FromResult(new Metadata());

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

    private async Task<CallOptions> BuildCallOptionsAsync(CancellationToken ct)
    {
        var md = await MetadataFactory(ct).ConfigureAwait(false);
        var deadline = DateTime.UtcNow.AddSeconds(30);
        return new CallOptions(headers: md, deadline: DateTime.UtcNow.AddSeconds(30), cancellationToken: ct);
    }

    // ---------- Vehicle ----------

    public async Task<DomainVehicle> CreateVehicleAsync(DomainVehicle vehicle, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        var response = await _vehicles.Value
            .CreateVehicleAsync(vehicle.ToCreateRequest(), opts)
            .ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task<DomainVehicle> GetVehicleAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id must be non-blank.", nameof(id));
        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        var response = await _vehicles.Value
            .GetVehicleAsync(new GetVehicleRequest { Id = id }, opts)
            .ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task<DomainVehicle> UpdateVehicleAsync(DomainVehicle vehicle, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        var response = await _vehicles.Value
            .UpdateVehicleAsync(vehicle.ToUpdateRequest(), opts)
            .ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task DeleteVehicleAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id must be non-blank.", nameof(id));
        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        await _vehicles.Value
            .DeleteVehicleAsync(new DeleteVehicleRequest { Id = id }, opts)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DomainVehicle>> ListVehiclesAsync(int pageSize, string? pageToken, CancellationToken ct = default)
    {
        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        var response = await _vehicles.Value
            .ListVehiclesAsync(
                new ListVehiclesRequest
                {
                    PageSize = pageSize,
                    PageToken = pageToken ?? string.Empty,
                },
                opts)
            .ConfigureAwait(false);
        var list = new List<DomainVehicle>(response.Vehicles.Count);
        foreach (var v in response.Vehicles) list.Add(v.ToDomain());
        return list;
    }

    // ---------- Charging ----------

    public async Task<DomainCharging> CreateChargingAsync(DomainCharging charging, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(charging);
        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        var response = await _chargings.Value
            .CreateChargingAsync(charging.ToCreateRequest(), opts)
            .ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task<DomainCharging> GetChargingAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id must be non-blank.", nameof(id));
        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        var response = await _chargings.Value
            .GetChargingAsync(new GetChargingRequest { Id = id }, opts)
            .ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task<DomainCharging> UpdateChargingAsync(DomainCharging charging, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(charging);
        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        var response = await _chargings.Value
            .UpdateChargingAsync(charging.ToUpdateRequest(), opts)
            .ConfigureAwait(false);
        return response.ToDomain();
    }

    public async Task DeleteChargingAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id must be non-blank.", nameof(id));
        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        await _chargings.Value
            .DeleteChargingAsync(new DeleteChargingRequest { Id = id }, opts)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DomainCharging>> ListChargingsAsync(
        string vehicleId,
        DateTimeOffset? startAfter = null,
        DateTimeOffset? startBefore = null,
        XYPortal.EvGRPC.Chargings.ChargerType? chargerType = null,
        string? sourceCategoryId = null,
        int pageSize = 100,
        string? pageToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(vehicleId))
            throw new ArgumentException("vehicleId must be non-blank.", nameof(vehicleId));
        if (pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageSize));

        var req = new ListChargingsRequest
        {
            VehicleId = vehicleId,
            PageSize = pageSize,
            PageToken = pageToken ?? string.Empty,
        };
        if (startAfter.HasValue) req.StartAfter = startAfter.Value.ToProtoTimestamp();
        if (startBefore.HasValue) req.StartBefore = startBefore.Value.ToProtoTimestamp();
        if (chargerType.HasValue) req.ChargerType = chargerType.Value.ToProto();
        if (!string.IsNullOrWhiteSpace(sourceCategoryId)) req.SourceCategoryId = sourceCategoryId;

        var opts = await BuildCallOptionsAsync(ct).ConfigureAwait(false);
        var response = await _chargings.Value.ListChargingsAsync(req, opts).ConfigureAwait(false);
        var list = new List<DomainCharging>(response.Chargings.Count);
        foreach (var c in response.Chargings) list.Add(c.ToDomain());
        return list;
    }

    public async Task<DomainCharging?> GetLatestChargingAsync(string vehicleId, CancellationToken ct = default)
    {
        var all = await ListChargingsAsync(vehicleId, pageSize: 500, pageToken: null, ct: ct).ConfigureAwait(false);
        DomainCharging? latest = null;
        foreach (var c in all)
        {
            if (latest is null || c.EndTime > latest.EndTime) latest = c;
        }
        return latest;
    }

    // ---------- Channel plumbing ----------

    private GrpcChannel BuildChannel()
    {
        // Token delivery is a *per-call* concern (CallCredentials → metadata);
        // TLS is a *channel* concern (SslCredentials). They were conflated
        // in Phase 1 (token non-empty → SslCredentials). That made any
        // http:// URL unreachable the moment a bearer token was configured.
        //
        // The correct combination is: pick the channel credential from the
        // URL scheme; if a token is present, attach it as a per-call
        // CallCredentials layer over whatever channel credential was chosen.
        var uri = new Uri(_options.Url);
        var hasToken = !string.IsNullOrWhiteSpace(_options.AccessToken);

        ChannelCredentials channelCred = uri.Scheme switch
        {
            "https" => ChannelCredentials.SecureSsl,
            _       => ChannelCredentials.Insecure,
        };

        if (hasToken)
        {
            var token = _options.AccessToken!;
            channelCred = ChannelCredentials.Create(
                channelCred,
                CallCredentials.FromInterceptor((_, metadata) =>
                {
                    metadata.Add("authorization", $"Bearer {token}");
                    return Task.CompletedTask;
                }));
        }

        var options = new GrpcChannelOptions
        {
            Credentials = channelCred,
            // 16 MiB matches proto defaults; explicit so future tuning has a hook.
            MaxReceiveMessageSize = 16 * 1024 * 1024,
            MaxSendMessageSize = 16 * 1024 * 1024,
        };
        // Insecure channel + per-RPC bearer requires this opt-in. It is a
        // documented escape hatch in grpc-dotnet for dev / behind-LB
        // scenarios (e.g. evGRpc behind nginx with h2c + JWT); production
        // HTTPS deployments never hit this branch because SecureSsl is
        // selected by the scheme switch above.
        if (hasToken && uri.Scheme != "https")
        {
            options.UnsafeUseInsecureChannelCallCredentials = true;
        }
        return GrpcChannel.ForAddress(_options.Url, options);
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
