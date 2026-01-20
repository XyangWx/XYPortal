using System;
using System.Collections.Generic;
using System.Text;
using XYPortal.Localization;
using Volo.Abp.Application.Services;

namespace XYPortal;

/* Inherit your application services from this class.
 */
public abstract class XYPortalAppService : ApplicationService
{
    protected XYPortalAppService()
    {
        LocalizationResource = typeof(XYPortalResource);
    }
}
