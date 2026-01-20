using XYPortal.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace XYPortal.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class XYPortalController : AbpControllerBase
{
    protected XYPortalController()
    {
        LocalizationResource = typeof(XYPortalResource);
    }
}
