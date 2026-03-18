using Localization.Resources.AbpUi;
using XYPortal.PasswordBook.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(PasswordBookApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class PasswordBookHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(PasswordBookHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<PasswordBookResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
