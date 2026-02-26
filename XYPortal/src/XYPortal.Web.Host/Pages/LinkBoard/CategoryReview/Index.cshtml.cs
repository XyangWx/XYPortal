using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.LinkBoard;
using XYPortal.LinkBoard.LinkCategories;
using XYPortal.LinkBoard.Permissions;

namespace XYPortal.Web.Pages.LinkBoard.CategoryReview;

[Authorize(LinkBoardPermissions.LinkCategoryReview)]
public class IndexModel : XYPortalPageModel
{
    private readonly ILinkCategoryReviewAppService _appService;

    public IndexModel(ILinkCategoryReviewAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetListAsync(GetLinkCategoryListInput input)
    {
        input.Status = ReviewStatus.Pending;
        var result = await _appService.GetListAsync(input);
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        await _appService.ReviewAsync(id, new ReviewInput { Status = ReviewStatus.Approved });
        return new NoContentResult();
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id, string? comment)
    {
        await _appService.ReviewAsync(id, new ReviewInput { Status = ReviewStatus.Rejected, ReviewComment = comment });
        return new NoContentResult();
    }
}
