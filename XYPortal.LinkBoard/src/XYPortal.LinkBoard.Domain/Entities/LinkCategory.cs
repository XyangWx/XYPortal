using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace XYPortal.LinkBoard.Entities;

public class LinkCategory : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; set; } = default!;

    public string? DisplayName { get; set; }

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

    /// <summary>
    /// Indicates if this is a system default category that cannot be modified or deleted.
    /// </summary>
    public bool IsDefault { get; set; }

    protected LinkCategory() { }

    public LinkCategory(Guid id, string name, bool isPublic = false) : base(id)
    {
        Name = name;
        IsPublic = isPublic;
        Status = ReviewStatus.Draft;
    }
}
