using XYPortal.EvGRPC.Localization;
using Volo.Abp.Application.Services;

namespace XYPortal.EvGRPC;

public abstract class EvGRPCAppService : ApplicationService
{
    protected EvGRPCAppService()
    {
        LocalizationResource = typeof(EvGRPCResource);
        ObjectMapperContext = typeof(EvGRPCApplicationModule);
    }
}
