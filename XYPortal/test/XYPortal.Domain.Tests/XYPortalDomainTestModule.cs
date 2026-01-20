using Volo.Abp.Modularity;

namespace XYPortal;

[DependsOn(
    typeof(XYPortalDomainModule),
    typeof(XYPortalTestBaseModule)
)]
public class XYPortalDomainTestModule : AbpModule
{

}
