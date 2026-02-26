using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using XYPortal.LinkBoard.Links;

namespace XYPortal.LinkBoard;

[RemoteService(Name = "Default")]
[Area("app")]
[Route("api/app/link")]
public class LinkController : LinkBoardController, ILinkAppService
{
    private readonly ILinkAppService _linkAppService;

    public LinkController(ILinkAppService linkAppService)
    {
        _linkAppService = linkAppService;
    }

    [HttpGet("{id}")]
    public virtual Task<LinkDto> GetAsync(Guid id)
    {
        return _linkAppService.GetAsync(id);
    }

    [HttpGet]
    public virtual Task<PagedResultDto<LinkDto>> GetListAsync([FromQuery] GetLinkListInput input)
    {
        return _linkAppService.GetListAsync(input);
    }

    [HttpPost]
    public virtual Task<LinkDto> CreateAsync([FromBody] CreateLinkDto input)
    {
        return _linkAppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<LinkDto> UpdateAsync(Guid id, [FromBody] UpdateLinkDto input)
    {
        return _linkAppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _linkAppService.DeleteAsync(id);
    }

    [HttpPost("{id}/submit")]
    public virtual Task SubmitAsync(Guid id)
    {
        return _linkAppService.SubmitAsync(id);
    }

    [HttpPost("{id}/withdraw")]
    public virtual Task WithdrawAsync(Guid id)
    {
        return _linkAppService.WithdrawAsync(id);
    }

    [HttpGet("public-board")]
    public virtual Task<PagedResultDto<LinkDto>> GetPublicBoardAsync([FromQuery] GetPublicBoardInput input)
    {
        return _linkAppService.GetPublicBoardAsync(input);
    }

    [HttpGet("max-links")]
    public virtual Task<int> GetMaxLinksAsync()
    {
        return _linkAppService.GetMaxLinksAsync();
    }
}
