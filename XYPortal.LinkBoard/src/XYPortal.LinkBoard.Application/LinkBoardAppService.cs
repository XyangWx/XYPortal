using XYPortal.LinkBoard.Localization;
using Volo.Abp.Application.Services;

namespace XYPortal.LinkBoard;

public abstract class LinkBoardAppService : ApplicationService
{
    protected LinkBoardAppService()
    {
        LocalizationResource = typeof(LinkBoardResource);
        ObjectMapperContext = typeof(LinkBoardApplicationModule);
    }
}
