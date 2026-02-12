using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace XYPortal.LinkBoard;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(LinkBoardDomainSharedModule)
)]
public class LinkBoardDomainModule : AbpModule
{

}
