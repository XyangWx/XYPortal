using XYPortal.EvGRPC.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace XYPortal.EvGRPC;

public abstract class EvGRPCController : AbpControllerBase
{
    protected EvGRPCController()
    {
        LocalizationResource = typeof(EvGRPCResource);
    }
}
