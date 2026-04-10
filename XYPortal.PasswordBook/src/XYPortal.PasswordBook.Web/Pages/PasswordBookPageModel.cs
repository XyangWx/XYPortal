using XYPortal.PasswordBook.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace XYPortal.PasswordBook.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class PasswordBookPageModel : AbpPageModel
{
    protected PasswordBookPageModel()
    {
        LocalizationResourceType = typeof(PasswordBookResource);
        ObjectMapperContext = typeof(PasswordBookWebModule);
    }
}
