using Volo.Abp.Modularity;

namespace XYPortal.LinkBoard;

[DependsOn(
    typeof(LinkBoardApplicationModule),
    typeof(LinkBoardDomainTestModule)
    )]
public class LinkBoardApplicationTestModule : AbpModule
{

}
