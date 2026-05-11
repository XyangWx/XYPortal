using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using XYPortal.LinkBoard.Entities;
using XYPortal.LinkBoard.EntityFrameworkCore.Repositories;
using XYPortal.LinkBoard.Repositories;

namespace XYPortal.LinkBoard.EntityFrameworkCore;

[DependsOn(
    typeof(LinkBoardDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class LinkBoardEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<LinkBoardDbContext>(options =>
        {
            options.AddDefaultRepositories<ILinkBoardDbContext>(includeAllEntities: true);

            options.AddRepository<LinkCategory, LinkCategoryRepository>();
            options.AddRepository<Link, LinkRepository>();
        });
    }
}
