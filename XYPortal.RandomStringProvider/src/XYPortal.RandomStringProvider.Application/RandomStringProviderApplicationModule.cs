using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Application;

namespace XYPortal.RandomStringProvider;

[DependsOn(
    typeof(RandomStringProviderDomainModule),
    typeof(RandomStringProviderApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule)
    )]
public class RandomStringProviderApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<RandomStringProviderApplicationModule>();
    }
}
