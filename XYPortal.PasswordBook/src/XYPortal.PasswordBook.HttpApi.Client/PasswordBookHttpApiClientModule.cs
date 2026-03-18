using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(PasswordBookApplicationContractsModule),
    typeof(AbpHttpClientModule))]
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
