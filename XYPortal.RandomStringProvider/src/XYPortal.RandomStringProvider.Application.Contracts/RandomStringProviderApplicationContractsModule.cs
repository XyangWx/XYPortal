using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace XYPortal.RandomStringProvider;

[DependsOn(
    typeof(RandomStringProviderDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
public class RandomStringProviderApplicationContractsModule : AbpModule
{

}
