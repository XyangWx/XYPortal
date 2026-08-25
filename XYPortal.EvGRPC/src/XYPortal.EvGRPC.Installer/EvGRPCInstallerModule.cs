using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace XYPortal.EvGRPC;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class EvGRPCInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<EvGRPCInstallerModule>();
        });
    }
}
