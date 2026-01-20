using Microsoft.Extensions.Localization;
using XYPortal.Localization;
using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace XYPortal;

[Dependency(ReplaceServices = true)]
public class XYPortalBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<XYPortalResource> _localizer;

    public XYPortalBrandingProvider(IStringLocalizer<XYPortalResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
