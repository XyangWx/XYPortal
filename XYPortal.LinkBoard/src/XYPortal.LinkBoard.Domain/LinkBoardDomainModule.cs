using Volo.Abp.Domain;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;

namespace XYPortal.LinkBoard;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpPermissionManagementDomainModule),
    typeof(AbpIdentityDomainModule),
    typeof(LinkBoardDomainSharedModule),
    typeof(LinkBoardApplicationContractsModule)
)]
public class LinkBoardDomainModule : AbpModule
{

}
