using XYPortal.LinkBoard.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace XYPortal.LinkBoard;

public abstract class LinkBoardController : AbpControllerBase
{
    protected LinkBoardController()
    {
        LocalizationResource = typeof(LinkBoardResource);
    }
}
