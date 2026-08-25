using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace XYPortal.EvGRPC;

[DependsOn(
    typeof(EvGRPCApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class EvGRPCHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(EvGRPCApplicationContractsModule).Assembly,
            EvGRPCRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<EvGRPCHttpApiClientModule>();
        });

    }
}
