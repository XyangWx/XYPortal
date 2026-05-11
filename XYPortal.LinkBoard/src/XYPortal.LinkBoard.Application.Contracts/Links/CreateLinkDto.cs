using System;
using System.ComponentModel.DataAnnotations;

namespace XYPortal.LinkBoard.Links;

public class CreateLinkDto
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

    public bool IsPublic { get; set; }
}
