using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Application;
using XYPortal.RandomStringProvider;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(PasswordBookDomainModule),
    typeof(PasswordBookApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(RandomStringProviderApplicationModule)
    )]
public class PasswordBookApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<PasswordBookApplicationModule>();
    }
}
