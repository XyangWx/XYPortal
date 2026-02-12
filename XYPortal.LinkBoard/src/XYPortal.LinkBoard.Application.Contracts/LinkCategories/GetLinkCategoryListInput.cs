using Volo.Abp.Application.Dtos;

namespace XYPortal.LinkBoard.LinkCategories;

public class GetLinkCategoryListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public ReviewStatus? Status { get; set; }
    public bool? IsPublic { get; set; }
}
