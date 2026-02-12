using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.LinkBoard.Links;
using XYPortal.LinkBoard.Permissions;

namespace XYPortal.Web.Pages.LinkBoard.Links;

[Authorize(LinkBoardPermissions.LinkManager)]
public class IndexModel : XYPortalPageModel
{
    private readonly ILinkAppService _appService;

    public IndexModel(ILinkAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetListAsync(GetLinkListInput input)
    {
        var result = await _appService.GetListAsync(input);
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _appService.DeleteAsync(id);
        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostSubmitAsync(Guid id)
    {
        await _appService.SubmitAsync(id);
        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostWithdrawAsync(Guid id)
    {
        await _appService.WithdrawAsync(id);
        return new NoContentResult();
    }
}
