using XYPortal.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace XYPortal.Web.Pages;

public abstract class XYPortalPageModel : AbpPageModel
{
    protected XYPortalPageModel()
    {
        LocalizationResourceType = typeof(XYPortalResource);
    }
}
