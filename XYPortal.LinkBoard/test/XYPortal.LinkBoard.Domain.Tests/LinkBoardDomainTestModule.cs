using Volo.Abp.Modularity;

namespace XYPortal.LinkBoard;

[DependsOn(
    typeof(LinkBoardDomainModule),
    typeof(LinkBoardTestBaseModule)
)]
public class LinkBoardDomainTestModule : AbpModule
{

}
