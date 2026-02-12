using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace XYPortal.LinkBoard;

[DependsOn(
    typeof(LinkBoardDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
public class LinkBoardApplicationContractsModule : AbpModule
{

}
