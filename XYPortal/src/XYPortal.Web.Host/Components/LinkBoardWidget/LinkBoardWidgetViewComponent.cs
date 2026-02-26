using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Features;
using XYPortal.LinkBoard.Features;
using XYPortal.LinkBoard.LinkCategories;
using XYPortal.LinkBoard.Links;

namespace XYPortal.Web.Components.LinkBoardWidget;

public class LinkBoardWidgetViewComponent : AbpViewComponent
{
    private readonly ILinkAppService _linkAppService;
    private readonly ILinkCategoryAppService _categoryAppService;
    private readonly IFeatureChecker _featureChecker;

    public LinkBoardWidgetViewComponent(
        ILinkAppService linkAppService,
        ILinkCategoryAppService categoryAppService,
        IFeatureChecker featureChecker)
    {
        _linkAppService = linkAppService;
        _categoryAppService = categoryAppService;
        _featureChecker = featureChecker;
    }

    public virtual async Task<IViewComponentResult> InvokeAsync()
    {
        var pageSize = await _featureChecker.GetAsync<int>(LinkBoardFeatures.MaxLinks);
        var categories = await _categoryAppService.GetPublicListAsync();
        var orderedCategories = categories.OrderBy(c => c.SortOrder).ToList();

        var linksByCategory = new Dictionary<Guid, List<LinkDto>>();
        var totalCountByCategory = new Dictionary<Guid, long>();

        foreach (var category in orderedCategories)
        {
            var result = await _linkAppService.GetPublicBoardAsync(new GetPublicBoardInput
            {
                CategoryId = category.Id,
                SkipCount = 0,
                MaxResultCount = pageSize
            });

            if (result.TotalCount > 0)
            {
                linksByCategory[category.Id] = result.Items.ToList();
                totalCountByCategory[category.Id] = result.TotalCount;
            }
        }

        // Only include categories that have links
        var model = new LinkBoardWidgetViewModel
        {
            Categories = orderedCategories.Where(c => totalCountByCategory.ContainsKey(c.Id)).ToList(),
            LinksByCategory = linksByCategory,
            TotalCountByCategory = totalCountByCategory,
            PageSize = pageSize
        };

        return View("~/Components/LinkBoardWidget/Default.cshtml", model);
    }
}

public class LinkBoardWidgetViewModel
{
    public List<LinkCategoryDto> Categories { get; set; } = [];
    public Dictionary<Guid, List<LinkDto>> LinksByCategory { get; set; } = [];
    public Dictionary<Guid, long> TotalCountByCategory { get; set; } = [];
    public int PageSize { get; set; }
}
