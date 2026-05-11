using Localization.Resources.AbpUi;
using XYPortal.RandomStringProvider.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace XYPortal.RandomStringProvider;

[DependsOn(
    typeof(RandomStringProviderApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class RandomStringProviderHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(RandomStringProviderHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<RandomStringProviderResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
