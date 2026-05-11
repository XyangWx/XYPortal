using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace XYPortal.LinkBoard;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class LinkBoardInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<LinkBoardInstallerModule>();
        });
    }
}
