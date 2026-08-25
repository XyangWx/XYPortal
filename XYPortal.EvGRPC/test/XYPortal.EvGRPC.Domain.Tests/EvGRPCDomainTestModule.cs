using Volo.Abp.Modularity;

namespace XYPortal.EvGRPC;

[DependsOn(
    typeof(EvGRPCDomainModule),
    typeof(EvGRPCTestBaseModule)
)]
public class EvGRPCDomainTestModule : AbpModule
{

}
