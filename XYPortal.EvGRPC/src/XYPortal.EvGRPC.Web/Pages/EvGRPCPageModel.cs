using XYPortal.EvGRPC.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace XYPortal.EvGRPC.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class EvGRPCPageModel : AbpPageModel
{
    protected EvGRPCPageModel()
    {
        LocalizationResourceType = typeof(EvGRPCResource);
        ObjectMapperContext = typeof(EvGRPCWebModule);
    }
}
