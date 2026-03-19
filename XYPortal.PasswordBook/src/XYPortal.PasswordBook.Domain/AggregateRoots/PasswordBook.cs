using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using XYPortal.PasswordBook.Entities;
using XYPortal.PasswordBook.Enums;
using XYPortal.PasswordBook.ValueObjects;

namespace XYPortal.PasswordBook.AggregateRoots;

/// <summary>
/// PasswordBook Aggregate Root
/// </summary>
public class PasswordBook : AggregateRoot<Guid>, ISoftDelete
{
    /// <summary>
    /// Owner User ID
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Password Format Requirements (stored as JSON)
    /// </summary>
    public string PasswordFormatJson { get; private set; }

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

    /// <summary>
    /// Deletion Time
    /// </summary>
    public DateTime? DeletionTime { get; private set; }

    private readonly List<PasswordEntry> _passwordEntries = new();
    public IReadOnlyCollection<PasswordEntry> PasswordEntries => _passwordEntries.AsReadOnly();

    private PasswordBook() { }

    internal PasswordBook(
        Guid id,
        Guid ownerId,
        string name,
        string? description,
        PasswordFormatRequirement passwordFormat) : base(id)
    {
        OwnerId = ownerId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        Description = description;
        PasswordFormatJson = SerializePasswordFormat(passwordFormat);
        CreationTime = DateTime.UtcNow;
        IsDeleted = false;
    }

    /// <summary>
    /// Get Password Format Requirements
    /// </summary>
    public PasswordFormatRequirement GetPasswordFormat()
    {
        return DeserializePasswordFormat(PasswordFormatJson);
    }

    /// <summary>
    /// Add PasswordEntry
    /// </summary>
    public PasswordEntry AddPasswordEntry(
        string title,
        bool hasUsername,
        string? username,
        PasswordType passwordType,
        PasswordWeakLevel? weakLevel,
        string password,
        string? remark = null)
    {
        CheckPasswordFormat(password, passwordType);

        var entry = new PasswordEntry(
            Guid.NewGuid(),
            Id,
            title,
            hasUsername,
            username,
            passwordType,
            weakLevel,
            password,
            remark
        );

        _passwordEntries.Add(entry);
        LastModificationTime = DateTime.UtcNow;

        return entry;
    }

    /// <summary>
    /// Update PasswordEntry Info
    /// </summary>
    public void UpdatePasswordEntry(
        Guid entryId,
        string title,
        bool hasUsername,
        string? username,
        string? remark)
    {
        var entry = _passwordEntries.FirstOrDefault(e => e.Id == entryId && !e.IsDeleted);
        if (entry == null)
            throw new EntityNotFoundException("Password entry not found");

        entry.UpdateInfo(title, hasUsername, username, remark);
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Update Password Value
    /// </summary>
    public void UpdatePasswordValue(Guid entryId, string newPassword)
    {
        var entry = _passwordEntries.FirstOrDefault(e => e.Id == entryId && !e.IsDeleted);
        if (entry == null)
            throw new EntityNotFoundException("Password entry not found");

        CheckPasswordFormat(newPassword, entry.PasswordType);
        entry.UpdatePassword(newPassword);
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove PasswordEntry (Soft Delete)
    /// </summary>
    public void RemovePasswordEntry(Guid entryId)
    {
        var entry = _passwordEntries.FirstOrDefault(e => e.Id == entryId && !e.IsDeleted);
        if (entry == null)
            throw new EntityNotFoundException("Password entry not found");

        entry.SoftDelete();
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore PasswordEntry
    /// </summary>
    public void RestorePasswordEntry(Guid entryId)
    {
        var entry = _passwordEntries.FirstOrDefault(e => e.Id == entryId && e.IsDeleted);
        if (entry == null)
            throw new EntityNotFoundException("Password entry not found or not deleted");

        entry.Restore();
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Hard Delete PasswordEntry
    /// </summary>
    public void HardDeletePasswordEntry(Guid entryId)
    {
        var entry = _passwordEntries.FirstOrDefault(e => e.Id == entryId);
        if (entry == null)
            throw new EntityNotFoundException("Password entry not found");

        _passwordEntries.Remove(entry);
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Update PasswordBook Info
    /// </summary>
    public void UpdateInfo(string name, string? description)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name));
        Description = description;
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Update Password Format Requirements
    /// </summary>
    public void UpdatePasswordFormat(PasswordFormatRequirement passwordFormat)
    {
        PasswordFormatJson = SerializePasswordFormat(passwordFormat);
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft Delete
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletionTime = DateTime.UtcNow;
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Restore
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletionTime = null;
        LastModificationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Hard Delete (Permanent Delete)
    /// </summary>
    public void HardDelete()
    {
        _passwordEntries.Clear();
    }

    private void CheckPasswordFormat(string password, PasswordType passwordType)
    {
        var format = GetPasswordFormat();
        
        if (format.AllowedType != passwordType)
            throw new BusinessException("PasswordBook:PasswordTypeNotAllowed")
                .WithData("AllowedType", format.AllowedType.ToString());

        var (isValid, errorMessage) = format.Validate(password);
        if (!isValid)
            throw new BusinessException("PasswordBook:InvalidPasswordFormat")
                .WithData("Error", errorMessage ?? "");
    }

    private static string SerializePasswordFormat(PasswordFormatRequirement requirement)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            requirement.MinLength,
            requirement.MaxLength,
            requirement.RequireUppercase,
            requirement.RequireLowercase,
            requirement.RequireDigit,
            requirement.RequireSpecialChar,
            requirement.SpecialChars,
            AllowedType = (int)requirement.AllowedType
        });
    }

    private static PasswordFormatRequirement DeserializePasswordFormat(string json)
    {
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        var allowedType = (PasswordType)root.GetProperty("AllowedType").GetInt32();

        return new PasswordFormatRequirement(
            minLength: root.GetProperty("MinLength").GetInt32(),
            maxLength: root.GetProperty("MaxLength").GetInt32(),
            requireUppercase: root.GetProperty("RequireUppercase").GetBoolean(),
            requireLowercase: root.GetProperty("RequireLowercase").GetBoolean(),
            requireDigit: root.GetProperty("RequireDigit").GetBoolean(),
            requireSpecialChar: root.GetProperty("RequireSpecialChar").GetBoolean(),
            specialChars: root.GetProperty("SpecialChars").GetString(),
            allowedType: allowedType
        );
    }
}
