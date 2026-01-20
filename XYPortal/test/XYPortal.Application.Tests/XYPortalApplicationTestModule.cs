using Volo.Abp.Modularity;

namespace XYPortal;

[DependsOn(
    typeof(XYPortalApplicationModule),
    typeof(XYPortalDomainTestModule)
)]
public class XYPortalApplicationTestModule : AbpModule
{

}
