using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace XYPortal.EvGRPC;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(EvGRPCDomainSharedModule)
)]
public class EvGRPCDomainModule : AbpModule
{

}
