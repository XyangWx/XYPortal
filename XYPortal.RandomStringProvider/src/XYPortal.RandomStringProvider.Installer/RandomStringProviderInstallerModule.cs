using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace XYPortal.RandomStringProvider;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class RandomStringProviderInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<RandomStringProviderInstallerModule>();
        });
    }
}
