using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using XYPortal.LinkBoard.LinkCategories;
using XYPortal.LinkBoard.Links;

namespace XYPortal.Web.Components.LinkBoardWidget;

public class LinkBoardWidgetViewComponent : AbpViewComponent
{
    private readonly ILinkAppService _linkAppService;
    private readonly ILinkCategoryAppService _categoryAppService;

    public LinkBoardWidgetViewComponent(
        ILinkAppService linkAppService,
        ILinkCategoryAppService categoryAppService)
    {
        _linkAppService = linkAppService;
        _categoryAppService = categoryAppService;
    }

    public virtual async Task<IViewComponentResult> InvokeAsync()
    {
        var links = await _linkAppService.GetPublicBoardAsync(new GetPublicBoardInput());
        var categoryIds = links.Select(l => l.CategoryId).Distinct().ToList();
        
        var categories = await _categoryAppService.GetPublicListAsync();

        var model = new LinkBoardWidgetViewModel
        {
            Categories = categories
                .Where(c => categoryIds.Contains(c.Id))
                .OrderBy(c => c.SortOrder)
                .ToList(),
            LinksByCategory = links
                .GroupBy(l => l.CategoryId)
                .ToDictionary(g => g.Key, g => g.OrderBy(l => l.SortOrder).ToList())
        };

        return View("~/Components/LinkBoardWidget/Default.cshtml", model);
    }
}

public class LinkBoardWidgetViewModel
{
    public List<LinkCategoryDto> Categories { get; set; } = [];
    public Dictionary<System.Guid, List<LinkDto>> LinksByCategory { get; set; } = [];
}
