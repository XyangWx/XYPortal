using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

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
            
            /* Add custom repositories here. Example:
            * options.AddRepository<Question, EfCoreQuestionRepository>();
            */
        });
    }
}
