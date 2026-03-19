using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;
using XYPortal.RandomStringProvider;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(PasswordBookApplicationContractsModule),
    typeof(AbpHttpClientModule),
    typeof(RandomStringProviderHttpApiClientModule)
)]
public class PasswordBookHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(PasswordBookApplicationContractsModule).Assembly,
            PasswordBookRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<PasswordBookHttpApiClientModule>();
        });

    }
}
