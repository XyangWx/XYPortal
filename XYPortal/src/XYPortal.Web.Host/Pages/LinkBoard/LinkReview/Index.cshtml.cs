using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.LinkBoard;
using XYPortal.LinkBoard.Links;
using XYPortal.LinkBoard.Permissions;

namespace XYPortal.Web.Pages.LinkBoard.LinkReview;

[Authorize(LinkBoardPermissions.LinkReview)]
public class IndexModel : XYPortalPageModel
{
    private readonly ILinkReviewAppService _appService;

    public IndexModel(ILinkReviewAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetListAsync(GetLinkListInput input)
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
