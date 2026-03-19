using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using XYPortal.PasswordBook.Enums;

namespace XYPortal.PasswordBook.Entities;

/// <summary>
/// PasswordEntry Entity
/// </summary>
public class PasswordEntry : Entity<Guid>
{
    /// <summary>
    /// PasswordBook ID
    /// </summary>
    public Guid PasswordBookId { get; private set; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Has Username
    /// </summary>
    public bool HasUsername { get; private set; }

    /// <summary>
    /// Username (valid when HasUsername is true)
    /// </summary>
    public string? Username { get; private set; }

    /// <summary>
    /// Password Type
    /// </summary>
    public PasswordType PasswordType { get; private set; }

    /// <summary>
    /// Weak Level (required for General password only)
    /// </summary>
    public PasswordWeakLevel? WeakLevel { get; private set; }

    /// <summary>
    /// Current Valid Password Value
    /// </summary>
    public string CurrentPassword { get; private set; }

    /// <summary>
    /// Remark
    /// </summary>
    public string? Remark { get; private set; }

    /// <summary>
    /// Creation Time
    /// </summary>
    public DateTime CreationTime { get; private set; }

    /// <summary>
    /// Last Modification Time
    /// </summary>
    public DateTime? LastModificationTime { get; private set; }

    /// <summary>
    /// Is Deleted
    /// </summary>
    public bool IsDeleted { get; private set; }

    private readonly List<PasswordHistory> _passwordHistories = new();
    public IReadOnlyCollection<PasswordHistory> PasswordHistories => _passwordHistories.AsReadOnly();

    private PasswordEntry() { }

    internal PasswordEntry(
        Guid id,
        Guid passwordBookId,
        string title,
        bool hasUsername,
        string? username,
        PasswordType passwordType,
        PasswordWeakLevel? weakLevel,
        string currentPassword,
        string? remark = null) : base(id)
    {
        PasswordBookId = passwordBookId;
        Title = Check.NotNullOrWhiteSpace(title, nameof(title));
        HasUsername = hasUsername;
        Username = hasUsername ? Check.NotNullOrWhiteSpace(username, nameof(username)) : null;
        PasswordType = passwordType;
        WeakLevel = passwordType == PasswordType.General ? weakLevel : null;
        CurrentPassword = Check.NotNullOrWhiteSpace(currentPassword, nameof(currentPassword));
        Remark = remark;
        CreationTime = DateTime.UtcNow;
        IsDeleted = false;

        _passwordHistories.Add(new PasswordHistory(
            Guid.NewGuid(),
            id,
            currentPassword,
            true,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Update Password
    /// </summary>
    public void UpdatePassword(string newPassword)
    {
        Check.NotNullOrWhiteSpace(newPassword, nameof(newPassword));

        foreach (var history in _passwordHistories)
        {
            history.MarkAsInvalid();
        }

        _passwordHistories.Add(new PasswordHistory(
            Guid.NewGuid(),
            Id,
            newPassword,
            true,
            DateTime.UtcNow
        ));

        CurrentPassword = newPassword;
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Update Entry Info
    /// </summary>
    public void UpdateInfo(string title, bool hasUsername, string? username, string? remark)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title));
        HasUsername = hasUsername;
        Username = hasUsername ? Check.NotNullOrWhiteSpace(username, nameof(username)) : null;
        Remark = remark;
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft Delete
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        LastModificationTime = DateTime.UtcNow;
    }
}
