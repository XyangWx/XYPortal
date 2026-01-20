using XYPortal.Localization;
using Volo.Abp.AspNetCore.Components;

namespace XYPortal.Blazor.WebApp;

public abstract class XYPortalComponentBase : AbpComponentBase
{
    protected XYPortalComponentBase()
    {
        LocalizationResource = typeof(XYPortalResource);
    }
}
