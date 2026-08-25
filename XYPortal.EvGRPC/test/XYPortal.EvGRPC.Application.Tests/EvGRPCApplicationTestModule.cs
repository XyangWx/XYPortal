using Volo.Abp.Modularity;

namespace XYPortal.EvGRPC;

[DependsOn(
    typeof(EvGRPCApplicationModule),
    typeof(EvGRPCDomainTestModule)
    )]
public class EvGRPCApplicationTestModule : AbpModule
{

}
