using System;
using Volo.Abp.Domain.Entities;

namespace XYPortal.PasswordBook.Entities;

/// <summary>
/// PasswordHistory Entity
/// </summary>
public class PasswordHistory : Entity<Guid>
{
    /// <summary>
    /// PasswordEntry ID
    /// </summary>
    public Guid PasswordEntryId { get; private set; }

    /// <summary>
    /// Password Value
    /// </summary>
    public string? PasswordValue { get; private set; }

    /// <summary>
    /// Is Current Valid Password
    /// </summary>
    public bool IsCurrent { get; private set; }

    /// <summary>
    /// Creation Time
    /// </summary>
    public DateTime CreationTime { get; private set; }

    private PasswordHistory() { }

    internal PasswordHistory(
        Guid id,
        Guid passwordEntryId,
        string passwordValue,
        bool isCurrent,
        DateTime creationTime) : base(id)
    {
        PasswordEntryId = passwordEntryId;
        PasswordValue = passwordValue;
        IsCurrent = isCurrent;
        CreationTime = creationTime;
    }

    /// <summary>
    /// Mark as Invalid Password
    /// </summary>
    internal void MarkAsInvalid()
    {
        IsCurrent = false;
    }
}
