using XYPortal.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace XYPortal.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class XYPortalPageModel : AbpPageModel
{
    protected XYPortalPageModel()
    {
        LocalizationResourceType = typeof(XYPortalResource);
    }
}
