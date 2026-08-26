namespace XYPortal.EvGRPC.EvGrpc;

/// <summary>
/// Configuration for <see cref="EvGrpcClient"/>. Bound from
/// <c>appsettings.json</c> at host startup via the ABP options
/// pattern (configured in Phase 3.5).
///
/// Example appsettings.json:
/// <code>
/// "EvGrpc": {
///   "Url": "https://evgrpc.internal:50051",
///   "AccessToken": "..."  // optional; populated from current user in Phase 3.5
/// }
/// </code>
/// </summary>
public sealed class EvGrpcOptions
{
    /// <summary>
    /// Full URL of the upstream evGRpc gRPC endpoint, including scheme
    /// and port. Examples:
    ///   <c>https://evgrpc.example.com:50051</c> (production)
    ///   <c>http://localhost:50051</c> (local dev container)
    /// </summary>
    /// <summary>appsettings.json section name: <c>EvGrpc</c>.</summary>
    public const string SectionName = "EvGrpc";

    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Optional bearer token used as <c>authorization</c> metadata on
    /// every call. In Phase 3.5 this is sourced from
    /// <c>IAbpCurrentPrincipalAccessor</c>; v1 allows a static fallback
    /// here for non-interactive calls.
    /// </summary>
    public string? AccessToken { get; set; }
}
