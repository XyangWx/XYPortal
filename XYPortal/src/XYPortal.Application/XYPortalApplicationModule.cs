using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Microsoft.Extensions.DependencyInjection;
using XYPortal.LinkBoard;
using XYPortal.RandomStringProvider;

namespace XYPortal;

[DependsOn(
    typeof(XYPortalDomainModule),
    typeof(LinkBoardApplicationModule),
    typeof(RandomStringProviderApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(XYPortalApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
// ReSharper disable once InconsistentNaming
public class XYPortalApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<XYPortalApplicationModule>();
    }
}
