using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using XYPortal.LinkBoard.LinkCategories;

namespace XYPortal.LinkBoard;

[RemoteService(Name = "Default")]
[Area("app")]
[Route("api/app/link-category")]
public class LinkCategoryController : LinkBoardController, ILinkCategoryAppService
{
    private readonly ILinkCategoryAppService _categoryAppService;

    public LinkCategoryController(ILinkCategoryAppService categoryAppService)
    {
        _categoryAppService = categoryAppService;
    }

    [HttpGet("{id}")]
    public virtual Task<LinkCategoryDto> GetAsync(Guid id)
    {
        return _categoryAppService.GetAsync(id);
    }

    [HttpGet]
    public virtual Task<PagedResultDto<LinkCategoryDto>> GetListAsync([FromQuery] GetLinkCategoryListInput input)
    {
        return _categoryAppService.GetListAsync(input);
    }

    [HttpGet("public-list")]
    public virtual Task<List<LinkCategoryDto>> GetPublicListAsync()
    {
        return _categoryAppService.GetPublicListAsync();
    }

    [HttpGet("private-list")]
    public virtual Task<List<LinkCategoryDto>> GetPrivateListAsync()
    {
        return _categoryAppService.GetPrivateListAsync();
    }

    [HttpPost]
    public virtual Task<LinkCategoryDto> CreateAsync([FromBody] CreateLinkCategoryDto input)
    {
        return _categoryAppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<LinkCategoryDto> UpdateAsync(Guid id, [FromBody] UpdateLinkCategoryDto input)
    {
        return _categoryAppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _categoryAppService.DeleteAsync(id);
    }

    [HttpPost("{id}/submit")]
    public virtual Task SubmitAsync(Guid id)
    {
        return _categoryAppService.SubmitAsync(id);
    }

    [HttpPost("{id}/withdraw")]
    public virtual Task WithdrawAsync(Guid id)
    {
        return _categoryAppService.WithdrawAsync(id);
    }
}
