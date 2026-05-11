using Localization.Resources.AbpUi;
using XYPortal.LinkBoard.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace XYPortal.LinkBoard;

[DependsOn(
    typeof(LinkBoardApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class LinkBoardHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(LinkBoardHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<LinkBoardResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
