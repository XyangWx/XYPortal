using Localization.Resources.AbpUi;
using XYPortal.EvGRPC.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace XYPortal.EvGRPC;

[DependsOn(
    typeof(EvGRPCApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class EvGRPCHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(EvGRPCHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<EvGRPCResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
