using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Features;
using Volo.Abp.Users;
using XYPortal.LinkBoard.Features;
using XYPortal.LinkBoard.LinkCategories;
using XYPortal.LinkBoard.Links;

namespace XYPortal.Web.Components.LinkBoardWidget;

public class LinkBoardWidgetViewComponent : AbpViewComponent
{
    private readonly ILinkAppService _linkAppService;
    private readonly ILinkCategoryAppService _categoryAppService;
    private readonly IFeatureChecker _featureChecker;
    private readonly ICurrentUser _currentUser;

    public LinkBoardWidgetViewComponent(
        ILinkAppService linkAppService,
        ILinkCategoryAppService categoryAppService,
        IFeatureChecker featureChecker,
        ICurrentUser currentUser)
    {
        _linkAppService = linkAppService;
        _categoryAppService = categoryAppService;
        _featureChecker = featureChecker;
        _currentUser = currentUser;
    }

    public virtual async Task<IViewComponentResult> InvokeAsync()
    {
        var pageSize = await _featureChecker.GetAsync<int>(LinkBoardFeatures.MaxLinks);

        // 1. 获取所有公开已审核的分类
        var publicCategories = await _categoryAppService.GetPublicListAsync();

        // 2. 如果用户已登录，获取自己的私有分类
        List<LinkCategoryDto> privateCategories = new();
        if (_currentUser.IsAuthenticated)
        {
            privateCategories = await _categoryAppService.GetPrivateListAsync();
        }

        // 3. 合并分类列表：公开分类 + 自己的私有分类，按 SortOrder 排序
        var allCategories = publicCategories
            .Concat(privateCategories.Where(p => !p.IsPublic))
            .OrderBy(c => c.SortOrder)
            .ToList();

        var linksByCategory = new Dictionary<Guid, List<LinkDto>>();
        var totalCountByCategory = new Dictionary<Guid, long>();

        foreach (var category in allCategories)
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
            Categories = allCategories.Where(c => totalCountByCategory.ContainsKey(c.Id)).ToList(),
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
