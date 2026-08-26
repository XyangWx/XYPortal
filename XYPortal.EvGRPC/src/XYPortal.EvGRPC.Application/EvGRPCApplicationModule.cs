using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using XYPortal.EvGRPC.EvGrpc;

namespace XYPortal.EvGRPC;

[DependsOn(
    typeof(EvGRPCDomainModule),
    typeof(EvGRPCApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule)
    )]
public class EvGRPCApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<EvGRPCApplicationModule>();

        // Single shared gRPC channel per process. Same lifetime as
        // EvGrpcClient: long-lived, multiplexed HTTP/2 keep-alive.
        context.Services.AddSingleton<EvGrpcClient>();

        // Bind EvGrpc: section of appsettings.json to EvGrpcOptions.
        // Jwt bearer token can be a static dev placeholder
        // (EvGrpc:AccessToken) — production will replace it via
        // IAbpCurrentPrincipalAccessor (Phase 4 follow-up).
        context.Services.AddOptions<EvGrpcOptions>()
            .BindConfiguration(EvGrpcOptions.SectionName);
    }

    // NOTE: We deliberately do NOT eagerly call EvGrpcClient.Connect()
    // in OnApplicationInitialization. The connect path validates
    // EvGrpc:Url and would crash hosts that don't ship the section
    // (e.g. the XYPortal.EvGRPC.EntityFrameworkCore.Tests fixture,
    // which loads the EvGRPC module to bootstrap ABP but never calls
    // any evGRpc endpoint). The channel is built lazily on first RPC.
}
