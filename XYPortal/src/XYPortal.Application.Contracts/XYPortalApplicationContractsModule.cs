using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using XYPortal.LinkBoard;
using XYPortal.PasswordBook;

namespace XYPortal;

[DependsOn(
    typeof(XYPortalDomainSharedModule),
    typeof(LinkBoardApplicationContractsModule),
    typeof(PasswordBookApplicationContractsModule),
    typeof(AbpAccountApplicationContractsModule),
    typeof(AbpFeatureManagementApplicationContractsModule),
    typeof(AbpIdentityApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationContractsModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(AbpTenantManagementApplicationContractsModule),
    typeof(AbpObjectExtendingModule)
)]
// ReSharper disable once InconsistentNaming
public class XYPortalApplicationContractsModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        XYPortalDtoExtensions.Configure();
    }
}
