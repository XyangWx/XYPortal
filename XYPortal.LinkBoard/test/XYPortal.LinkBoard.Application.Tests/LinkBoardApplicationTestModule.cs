using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using XYPortal.LinkBoard.EntityFrameworkCore;

namespace XYPortal.LinkBoard;

[DependsOn(
    typeof(LinkBoardApplicationModule),
    typeof(LinkBoardDomainTestModule),
    typeof(LinkBoardEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule)
)]
public class LinkBoardApplicationTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(x =>
            {
                x.DbContextOptions.UseInMemoryDatabase("TestDb");
            });
        });
    }
}
