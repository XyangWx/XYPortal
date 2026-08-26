using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using XYPortal.EvGRPC.Application.EvGrpc;
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

        // Bind EvGrpc: section of appsettings.json to EvGrpcOptions.
        // The URL is mandatory; the AccessToken is the dev fallback used
        // when no user is bound (background jobs, EFCore test fixture,
        // etc.). Real authenticated requests go through the decorator below.
        context.Services.AddOptions<EvGrpcOptions>()
            .BindConfiguration(EvGrpcOptions.SectionName);

        // EvGrpcClient is the singleton carrying the long-lived gRPC
        // channel. Its MetadataFactory hook is overridden per request
        // by the Decorator below to inject the current user's bearer
        // token.
        context.Services.AddSingleton<EvGrpcClient>();

        // IEvGrpcClient is what AppServices depend on. It is registered
        // as Scoped because the Decorator holds ICurrentUser and IServiceScope
        // lookups (or directly ICurrentUser when scoped). The decorator's
        // ctor runs at request time, replacing the inner client's
        // MetadataFactory hook before any RPC fires.
        context.Services.AddScoped<IEvGrpcClient, EvGrpcClientDecorator>();
    }

    // NOTE: We deliberately do NOT eagerly call EvGrpcClient.Connect()
    // in OnApplicationInitialization. The connect path validates
    // EvGrpc:Url and would crash hosts that don't ship the section
    // (e.g. the XYPortal.EvGRPC.EntityFrameworkCore.Tests fixture,
    // which loads the EvGRPC module to bootstrap ABP but never calls
    // any evGRpc endpoint). The channel is built lazily on first RPC.
}
