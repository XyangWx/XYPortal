using XYPortal.RandomStringProvider.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace XYPortal.RandomStringProvider.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class RandomStringProviderPageModel : AbpPageModel
{
    protected RandomStringProviderPageModel()
    {
        LocalizationResourceType = typeof(RandomStringProviderResource);
        ObjectMapperContext = typeof(RandomStringProviderWebModule);
    }
}
