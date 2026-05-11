using System;
using Volo.Abp.Application.Dtos;

namespace XYPortal.LinkBoard.Links;

public class GetLinkListInput : PagedAndSortedResultRequestDto
{
    public Guid? CategoryId { get; set; }
    public string? Filter { get; set; }
    public ReviewStatus? Status { get; set; }
    public bool? IsPublic { get; set; }
}
