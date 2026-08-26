using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;
using XYPortal.EvGRPC.Application.EvGrpc;
using XYPortal.EvGRPC.EvGrpc;

namespace XYPortal.EvGRPC.EvGrpc.Tests;

/// <summary>
/// Verifies the per-call bearer-token resolution order of
/// <see cref="EvGrpcClientDecorator"/>. We construct a real
/// <see cref="EvGrpcClient"/> (its lazy channel is never fired in
/// these tests) and inspect the <c>MetadataFactory</c> hook the
/// decorator installs — that hook is where the token-resolution
/// policy actually lives.
/// </summary>
public class EvGrpcClientDecorator_Tests
{
    [Theory]
    [InlineData("access_token", "token-via-access_token")]
    [InlineData("oidc_access_token", "token-via-oidc")]
    [InlineData("openid_access_token", "token-via-openid")]
    public async Task Token_is_resolved_from_claim_with_priority(string claimName, string expected)
    {
        var (decorator, inner) = BuildDecorator(
            configuredStatic: "token-static",
            claimName: claimName,
            claimValue: expected);

        // Decorator overwrote inner.MetadataFactory in its ctor —
        // invoke it and observe the Metadata it produces.
        var metadata = await inner.MetadataFactory(CancellationToken.None);

        metadata.Get("authorization").Value.ShouldBe($"Bearer {expected}");
    }

    [Fact]
    public async Task Falls_back_to_static_EvGrpcOptions_token_when_no_claim_present()
    {
        var (decorator, inner) = BuildDecorator(
            configuredStatic: "static-fallback",
            claimName: null,
            claimValue: null);

        var metadata = await inner.MetadataFactory(CancellationToken.None);

        metadata.Get("authorization").Value.ShouldBe("Bearer static-fallback");
    }

    [Fact]
    public async Task Empty_token_when_neither_claim_nor_static_configured()
    {
        var (decorator, inner) = BuildDecorator(
            configuredStatic: null,
            claimName: null,
            claimValue: null);

        var metadata = await inner.MetadataFactory(CancellationToken.None);

        // No "authorization" header — the upstream will return 401 and
        // the AppService layer will translate that.
        metadata.Count.ShouldBe(0);
    }

    // ---------- helpers ----------

    private static (EvGrpcClientDecorator decorator, EvGrpcClient client)
        BuildDecorator(string? configuredStatic, string? claimName, string? claimValue)
    {
        var opts = Options.Create(new EvGrpcOptions
        {
            Url = "http://127.0.0.1:80",
            AccessToken = configuredStatic ?? string.Empty,
        });

        var client = new EvGrpcClient(opts);   // real ctor; lazy channel untouched

        var currentUser = Substitute.For<Volo.Abp.Users.ICurrentUser>();
        if (claimName is not null && claimValue is not null)
        {
            currentUser.FindClaim(claimName).Returns(
                new System.Security.Claims.Claim(claimName, claimValue));
        }
        else
        {
            currentUser.FindClaim(Arg.Any<string>()).Returns((System.Security.Claims.Claim?)null);
        }

        var decorator = new EvGrpcClientDecorator(client, currentUser, opts);
        return (decorator, client);
    }
}
