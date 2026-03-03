using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace XYPortal.RandomStringProvider;

[DependsOn(
    typeof(RandomStringProviderApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class RandomStringProviderHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(RandomStringProviderApplicationContractsModule).Assembly,
            RandomStringProviderRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<RandomStringProviderHttpApiClientModule>();
        });

    }
}
