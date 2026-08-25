using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace XYPortal.EvGRPC.EntityFrameworkCore;

[DependsOn(
    typeof(EvGRPCDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class EvGRPCEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<EvGRPCDbContext>(options =>
        {
            options.AddDefaultRepositories<IEvGRPCDbContext>(includeAllEntities: true);
            
            /* Add custom repositories here. Example:
            * options.AddRepository<Question, EfCoreQuestionRepository>();
            */
        });
    }
}
