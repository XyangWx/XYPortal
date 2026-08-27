using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;
using XYPortal.EvGRPC.Chargings;
using XYPortal.EvGRPC.EvGrpc;
using DomainVehicle = XYPortal.EvGRPC.Vehicles.Vehicle;
using DomainCharging = XYPortal.EvGRPC.Chargings.Charging;

namespace XYPortal.EvGRPC.Application.EvGrpc;

/// <summary>
/// Per-call bearer-token injector for <see cref="global::XYPortal.EvGRPC.EvGrpc.EvGrpcClient"/>.
///
/// Replaces the underlying client's <c>MetadataFactory</c> hook with
/// one that reads the access token from the current request's
/// <see cref="ICurrentUser"/> claims. Falls back to the static
/// <c>EvGrpc:AccessToken</c> (configured for dev) when no user is
/// bound to the current DI scope (background jobs, EFCore test
/// fixture, etc.).
///
/// Token resolution order:
///
///   1. <c>access_token</c> claim (ABP standard name)
///   2. <c>oidc_access_token</c> claim (OpenIddict common name)
///   3. <c>openid_access_token</c> claim (legacy fallback)
///   4. <c>EvGrpcOptions.AccessToken</c> (static, dev only)
///   5. empty string (call will be rejected by upstream with 401)
///
/// The hook runs inside a per-RPC gRPC interceptor. It only ever
/// resolves against the active DI scope (because <see cref="EvGrpcClient"/>
/// is singleton and the interceptor captures the factory at call
/// time), so each request gets its own token without a service-locator
/// hack.
/// </summary>
public sealed class EvGrpcClientDecorator : IEvGrpcClient, IDisposable
{
    private readonly EvGrpcClient _inner;
    private readonly ICurrentUser _currentUser;
    private readonly EvGrpcOptions _staticOptions;
    
    public EvGrpcClientDecorator(
        EvGrpcClient inner,
        ICurrentUser currentUser,
        IOptions<EvGrpcOptions> staticOptions)
    {
        _inner = inner;
        _currentUser = currentUser;
        _staticOptions = staticOptions.Value;

        // Override the inner client's per-call metadata factory. Because
        // the factory closure runs inside the gRPC call's interceptor
        // (which executes on the DI scope of the originating request),
        // ICurrentUser here resolves the **logged-in** user, not some
        // singleton snapshot.
        _inner.MetadataFactory = ResolveMetadataAsync;
    }

    public void Dispose()
    {
        // Reset to the default no-op so we don't leak the closure past
        // the decorator's lifetime (matters in tests; production
        // singletons live for the process lifetime anyway).
        _inner.MetadataFactory = static _ => Task.FromResult(new Metadata());
    }

    private Task<Metadata> ResolveMetadataAsync(CancellationToken _)
    {
        // Probe several claim-name conventions; XYPortal modules differ
        // depending on which OpenIddict integration shipped. The first
        // hit wins; otherwise the static fallback applies.
        var token = TryClaim("AccessToken")             // ASP.NET Core OpenIdConnect / cookie auth convention
                    ?? TryClaim("access_token")         // OpenIddict RFC-style
                    ?? TryClaim("oidc_access_token")     // OpenIddict common name
                    ?? TryClaim("openid_access_token")   // legacy fallback
                    ?? _staticOptions.AccessToken
                    ?? string.Empty;

        var md = new Metadata();
        if (!string.IsNullOrEmpty(token))
        {
            md.Add("authorization", $"Bearer {token}");
        }
        return Task.FromResult(md);
    }

    private string? TryClaim(string name)
    {
        // ICurrentUser is null when no user is bound (background, tests).
        if (_currentUser is null) return null;
        var claim = _currentUser.FindClaim(name);
        return string.IsNullOrWhiteSpace(claim?.Value) ? null : claim.Value;
    }

    // ---------- Vehicle ----------

    public Task<DomainVehicle> CreateVehicleAsync(DomainVehicle vehicle, CancellationToken ct = default)
        => _inner.CreateVehicleAsync(vehicle, ct);

    public Task<DomainVehicle> GetVehicleAsync(string id, CancellationToken ct = default)
        => _inner.GetVehicleAsync(id, ct);

    public Task<DomainVehicle> UpdateVehicleAsync(DomainVehicle vehicle, CancellationToken ct = default)
        => _inner.UpdateVehicleAsync(vehicle, ct);

    public Task DeleteVehicleAsync(string id, CancellationToken ct = default)
        => _inner.DeleteVehicleAsync(id, ct);

    public Task<IReadOnlyList<DomainVehicle>> ListVehiclesAsync(int pageSize, string? pageToken, CancellationToken ct = default)
        => _inner.ListVehiclesAsync(pageSize, pageToken, ct);

    // ---------- Charging ----------

    public Task<DomainCharging> CreateChargingAsync(DomainCharging charging, CancellationToken ct = default)
        => _inner.CreateChargingAsync(charging, ct);

    public Task<DomainCharging> GetChargingAsync(string id, CancellationToken ct = default)
        => _inner.GetChargingAsync(id, ct);

    public Task<DomainCharging> UpdateChargingAsync(DomainCharging charging, CancellationToken ct = default)
        => _inner.UpdateChargingAsync(charging, ct);

    public Task DeleteChargingAsync(string id, CancellationToken ct = default)
        => _inner.DeleteChargingAsync(id, ct);

    public Task<IReadOnlyList<DomainCharging>> ListChargingsAsync(
        string vehicleId,
        DateTimeOffset? startAfter = null,
        DateTimeOffset? startBefore = null,
        ChargerType? chargerType = null,
        string? sourceCategoryId = null,
        int pageSize = 100,
        string? pageToken = null,
        CancellationToken ct = default)
        => _inner.ListChargingsAsync(vehicleId, startAfter, startBefore, chargerType, sourceCategoryId, pageSize, pageToken, ct);

    public Task<DomainCharging?> GetLatestChargingAsync(string vehicleId, CancellationToken ct = default)
        => _inner.GetLatestChargingAsync(vehicleId, ct);
}
