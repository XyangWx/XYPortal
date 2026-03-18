using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(PasswordBookDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
public class PasswordBookApplicationContractsModule : AbpModule
{

}
