using System;

namespace XYPortal.LinkBoard.LinkCategories;

public class LinkCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublic { get; set; }
    public ReviewStatus Status { get; set; }
    public string? ReviewComment { get; set; }
    public Guid? DraftOfId { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTime CreationTime { get; set; }
}
