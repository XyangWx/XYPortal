using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace XYPortal.LinkBoard;

[DependsOn(
    typeof(LinkBoardApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class LinkBoardHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(LinkBoardApplicationContractsModule).Assembly,
            LinkBoardRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<LinkBoardHttpApiClientModule>();
        });

    }
}
