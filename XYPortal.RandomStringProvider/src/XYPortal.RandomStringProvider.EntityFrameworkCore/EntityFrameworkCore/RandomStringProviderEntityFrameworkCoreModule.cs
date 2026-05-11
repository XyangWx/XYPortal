using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace XYPortal.RandomStringProvider.EntityFrameworkCore;

[DependsOn(
    typeof(RandomStringProviderDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class RandomStringProviderEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<RandomStringProviderDbContext>(options =>
        {
            options.AddDefaultRepositories<IRandomStringProviderDbContext>(includeAllEntities: true);
            
            /* Add custom repositories here. Example:
            * options.AddRepository<Question, EfCoreQuestionRepository>();
            */
        });
    }
}
