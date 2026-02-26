using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace XYPortal.LinkBoard.Entities;

public class Link : FullAuditedAggregateRoot<Guid>
{
    public Guid CategoryId { get; set; }

    public string Title { get; set; } = default!;

    public string Url { get; set; } = default!;

    public string? Description { get; set; }

    public string? Icon { get; set; }

    public int SortOrder { get; set; }

    public bool IsPublic { get; set; }

    public ReviewStatus Status { get; set; }

    public string? ReviewComment { get; set; }

    /// <summary>
    /// If this record is a draft of an approved record, this points to the original.
    /// Used for edit-then-approve workflow: original stays visible until draft is approved.
    /// </summary>
    public Guid? DraftOfId { get; set; }

    public virtual LinkCategory Category { get; set; } = default!;

    protected Link() { }

    public Link(Guid id, Guid categoryId, string title, string url, bool isPublic = false) : base(id)
    {
        CategoryId = categoryId;
        Title = title;
        Url = url;
        IsPublic = isPublic;
        Status = ReviewStatus.Draft;
    }
}
