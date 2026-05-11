using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;
using XYPortal.RandomStringProvider;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(PasswordBookDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule),
    typeof(RandomStringProviderApplicationContractsModule)
    )]
public class PasswordBookApplicationContractsModule : AbpModule
{

}
