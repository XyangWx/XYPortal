using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.LinkBoard.LinkCategories;
using XYPortal.LinkBoard.Permissions;

namespace XYPortal.Web.Pages.LinkBoard.Categories;

[Authorize(LinkBoardPermissions.LinkCategoryManager)]
public class IndexModel : XYPortalPageModel
{
    private readonly ILinkCategoryAppService _appService;

    public IndexModel(ILinkCategoryAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetListAsync(GetLinkCategoryListInput input)
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
