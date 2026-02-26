using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using XYPortal.LinkBoard.LinkCategories;

namespace XYPortal.LinkBoard;

[RemoteService(Name = "Default")]
[Area("app")]
[Route("api/app/link-category-review")]
public class LinkCategoryReviewController : LinkBoardController, ILinkCategoryReviewAppService
{
    private readonly ILinkCategoryReviewAppService _reviewAppService;

    public LinkCategoryReviewController(ILinkCategoryReviewAppService reviewAppService)
    {
        _reviewAppService = reviewAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<LinkCategoryDto>> GetListAsync([FromQuery] GetLinkCategoryListInput input)
    {
        return _reviewAppService.GetListAsync(input);
    }

    [HttpPost("{id}/review")]
    public virtual Task ReviewAsync([FromRoute] Guid id, [FromBody] ReviewInput input)
    {
        return _reviewAppService.ReviewAsync(id, input);
    }
}
