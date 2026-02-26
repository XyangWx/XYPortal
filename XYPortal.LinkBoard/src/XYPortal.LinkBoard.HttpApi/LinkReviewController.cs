using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using XYPortal.LinkBoard.Links;

namespace XYPortal.LinkBoard;

[RemoteService(Name = "Default")]
[Area("app")]
[Route("api/app/link-review")]
public class LinkReviewController : LinkBoardController, ILinkReviewAppService
{
    private readonly ILinkReviewAppService _reviewAppService;

    public LinkReviewController(ILinkReviewAppService reviewAppService)
    {
        _reviewAppService = reviewAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<LinkDto>> GetListAsync([FromQuery] GetLinkListInput input)
    {
        return _reviewAppService.GetListAsync(input);
    }

    [HttpPost("{id}/review")]
    public virtual Task ReviewAsync([FromRoute] Guid id, [FromBody] ReviewInput input)
    {
        return _reviewAppService.ReviewAsync(id, input);
    }
}
