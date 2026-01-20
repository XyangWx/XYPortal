using XYPortal.Localization;
using Volo.Abp.AspNetCore.Components;

namespace XYPortal.Blazor.WebApp.Tiered;

public abstract class XYPortalComponentBase : AbpComponentBase
{
    protected XYPortalComponentBase()
    {
        LocalizationResource = typeof(XYPortalResource);
    }
}
