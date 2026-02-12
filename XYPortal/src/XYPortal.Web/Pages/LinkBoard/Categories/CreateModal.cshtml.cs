using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.LinkBoard;
using XYPortal.LinkBoard.LinkCategories;
using XYPortal.LinkBoard.Permissions;

namespace XYPortal.Web.Pages.LinkBoard.Categories;

[Authorize(LinkBoardPermissions.LinkCategoryCreate)]
public class CreateModalModel : XYPortalPageModel
{
    [BindProperty]
    public CreateViewModel Input { get; set; } = new();

    private readonly ILinkCategoryAppService _appService;

    public CreateModalModel(ILinkCategoryAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
        Input = new CreateViewModel();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new CreateLinkCategoryDto
        {
            Name = Input.Name,
            DisplayName = Input.DisplayName,
            Description = Input.Description,
            Icon = Input.Icon,
            SortOrder = Input.SortOrder,
            IsPublic = Input.IsPublic
        };

        await _appService.CreateAsync(dto);
        return NoContent();
    }

    public class CreateViewModel
    {
        [Required]
        [MaxLength(LinkBoardConsts.CategoryNameMaxLength)]
        public string Name { get; set; } = default!;

        [MaxLength(LinkBoardConsts.CategoryDisplayNameMaxLength)]
        public string? DisplayName { get; set; }

        [MaxLength(LinkBoardConsts.CategoryDescriptionMaxLength)]
        public string? Description { get; set; }

        [MaxLength(LinkBoardConsts.CategoryIconMaxLength)]
        public string? Icon { get; set; }

        public int SortOrder { get; set; }

        public bool IsPublic { get; set; }
    }
}
