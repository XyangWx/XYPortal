using XYPortal.Localization;
using Volo.Abp.AspNetCore.Components;

namespace XYPortal.Blazor.Server.Tiered;

public abstract class XYPortalComponentBase : AbpComponentBase
{
    protected XYPortalComponentBase()
    {
        LocalizationResource = typeof(XYPortalResource);
    }
}
