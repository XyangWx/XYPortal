using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace XYPortal.EvGRPC;

[DependsOn(
    typeof(EvGRPCDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
public class EvGRPCApplicationContractsModule : AbpModule
{

}
