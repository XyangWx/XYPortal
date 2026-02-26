using System.ComponentModel.DataAnnotations;

namespace XYPortal.LinkBoard.LinkCategories;

public class UpdateLinkCategoryDto
{
    [MaxLength(LinkBoardConsts.CategoryDisplayNameMaxLength)]
    public string? DisplayName { get; set; }

    [MaxLength(LinkBoardConsts.CategoryDescriptionMaxLength)]
    public string? Description { get; set; }

    [MaxLength(LinkBoardConsts.CategoryIconMaxLength)]
    public string? Icon { get; set; }

    public int SortOrder { get; set; }
}
