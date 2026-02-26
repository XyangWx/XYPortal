using XYPortal.Localization;
using Volo.Abp.Application.Services;

namespace XYPortal;

/* Inherit your application services from this class.
 */
// ReSharper disable once InconsistentNaming
public abstract class XYPortalAppService : ApplicationService
{
    protected XYPortalAppService()
    {
        LocalizationResource = typeof(XYPortalResource);
    }
}
