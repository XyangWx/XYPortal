using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using XYPortal.LinkBoard;
using XYPortal.LinkBoard.LinkCategories;
using XYPortal.LinkBoard.Links;
using XYPortal.LinkBoard.Permissions;

namespace XYPortal.Web.Pages.LinkBoard.Links;

[Authorize(LinkBoardPermissions.LinkModify)]
public class EditModalModel : XYPortalPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public EditViewModel Input { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = [];

    private readonly ILinkAppService _linkAppService;
    private readonly ILinkCategoryAppService _categoryAppService;

    public EditModalModel(ILinkAppService linkAppService, ILinkCategoryAppService categoryAppService)
    {
        _linkAppService = linkAppService;
        _categoryAppService = categoryAppService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _linkAppService.GetAsync(Id);
        Input = new EditViewModel
        {
            CategoryId = dto.CategoryId,
            Title = dto.Title,
            Url = dto.Url,
            Description = dto.Description ?? string.Empty,
            Icon = dto.Icon ?? string.Empty,
            SortOrder = dto.SortOrder
        };
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new UpdateLinkDto
        {
            CategoryId = Input.CategoryId,
            Title = Input.Title,
            Url = Input.Url,
            Description = Input.Description,
            Icon = Input.Icon,
            SortOrder = Input.SortOrder
        };

        await _linkAppService.UpdateAsync(Id, dto);
        return NoContent();
    }

    private async Task LoadCategoriesAsync()
    {
        var result = await _categoryAppService.GetListAsync(new GetLinkCategoryListInput { MaxResultCount = 1000 });
        Categories = result.Items
            .Select(c => new SelectListItem(c.DisplayName ?? c.Name, c.Id.ToString()))
            .ToList();
    }

    public class EditViewModel
    {
        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [MaxLength(LinkBoardConsts.LinkTitleMaxLength)]
        public string Title { get; set; } = default!;

        [Required]
        [MaxLength(LinkBoardConsts.LinkUrlMaxLength)]
        [Url]
        public string Url { get; set; } = default!;

        [MaxLength(LinkBoardConsts.LinkDescriptionMaxLength)]
        public string? Description { get; set; }

        [MaxLength(LinkBoardConsts.LinkIconMaxLength)]
        public string? Icon { get; set; }

        public int SortOrder { get; set; }
    }
}
