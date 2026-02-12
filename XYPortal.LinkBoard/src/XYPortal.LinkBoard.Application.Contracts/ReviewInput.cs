using System.ComponentModel.DataAnnotations;

namespace XYPortal.LinkBoard;

public class ReviewInput
{
    [Required]
    public ReviewStatus Status { get; set; }

    [MaxLength(LinkBoardConsts.ReviewCommentMaxLength)]
    public string? ReviewComment { get; set; }
}
