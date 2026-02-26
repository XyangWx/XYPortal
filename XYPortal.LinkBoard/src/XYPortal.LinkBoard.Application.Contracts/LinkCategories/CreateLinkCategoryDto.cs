using System.ComponentModel.DataAnnotations;

namespace XYPortal.LinkBoard.LinkCategories;

public class CreateLinkCategoryDto
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
