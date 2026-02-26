using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.LinkBoard;
using XYPortal.LinkBoard.LinkCategories;
using XYPortal.LinkBoard.Permissions;

namespace XYPortal.Web.Pages.LinkBoard.Categories;

[Authorize(LinkBoardPermissions.LinkCategoryModify)]
public class EditModalModel : XYPortalPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public EditViewModel Input { get; set; } = new();

    private readonly ILinkCategoryAppService _appService;

    public EditModalModel(ILinkCategoryAppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _appService.GetAsync(Id);
        Input = new EditViewModel
        {
            Name = dto.Name,
            DisplayName = dto.DisplayName ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Icon = dto.Icon ?? string.Empty,
            SortOrder = dto.SortOrder
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new UpdateLinkCategoryDto
        {
            DisplayName = Input.DisplayName,
            Description = Input.Description,
            Icon = Input.Icon,
            SortOrder = Input.SortOrder
        };

        await _appService.UpdateAsync(Id, dto);
        return NoContent();
    }

    public class EditViewModel
    {
        public string Name { get; set; } = default!;

        [MaxLength(LinkBoardConsts.CategoryDisplayNameMaxLength)]
        public string? DisplayName { get; set; }

        [MaxLength(LinkBoardConsts.CategoryDescriptionMaxLength)]
        public string? Description { get; set; }

        [MaxLength(LinkBoardConsts.CategoryIconMaxLength)]
        public string? Icon { get; set; }

        public int SortOrder { get; set; }
    }
}
