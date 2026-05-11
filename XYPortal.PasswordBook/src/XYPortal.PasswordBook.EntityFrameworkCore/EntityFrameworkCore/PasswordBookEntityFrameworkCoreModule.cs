using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace XYPortal.PasswordBook.EntityFrameworkCore;

[DependsOn(
    typeof(PasswordBookDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class PasswordBookEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<PasswordBookDbContext>(options =>
        {
            options.AddDefaultRepositories<IPasswordBookDbContext>(includeAllEntities: true);
            
            /* Add custom repositories here. Example:
            * options.AddRepository<Question, EfCoreQuestionRepository>();
            */
        });
    }
}
