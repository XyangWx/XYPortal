using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class PasswordBookInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<PasswordBookInstallerModule>();
        });
    }
}
