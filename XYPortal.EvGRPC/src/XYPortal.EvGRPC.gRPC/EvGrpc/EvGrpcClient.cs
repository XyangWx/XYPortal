using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace XYPortal.EvGRPC.EvGrpc;

/// <summary>
/// Thin skeleton for the upstream evGRpc gRPC client. Phase 1 ships
/// only the connection plumbing — a single <see cref="GrpcChannel"/>
/// shared across all generated service stubs. Phase 2.4 replaces
/// this class with 8 typed CRUD methods (Vehicle / Charging) that
/// call the generated stubs and map proto types to Domain entities.
///
/// The class is registered as a singleton (one channel per process)
/// because <c>GrpcChannel</c> is designed for long-lived reuse — a
/// new channel per RPC would defeat the multiplexed HTTP/2 keep-alive.
/// </summary>
public sealed class EvGrpcClient : IDisposable
{
    private readonly EvGrpcOptions _options;
    private readonly ILogger<EvGrpcClient> _logger;
    private readonly Lazy<GrpcChannel> _channel;
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
    }

    /// <summary>
    /// Eagerly initialize the underlying gRPC channel. Idempotent and
    /// cheap on subsequent calls — the lazy channel is built once per
    /// process. Wire-level smoke test for Phase 1: this method must
    /// not throw when <c>EvGrpc:Url</c> is well-formed.
    /// </summary>
    public bool Connect()
    {
        _ = _channel.Value;
        _logger.LogInformation("EvGRPC channel initialized for {Url}", _options.Url);
        return true;
    }

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
