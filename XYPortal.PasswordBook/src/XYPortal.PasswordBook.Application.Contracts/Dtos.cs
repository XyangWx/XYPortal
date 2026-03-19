using System;
using XYPortal.PasswordBook.Enums;

namespace XYPortal.PasswordBook.Application.Contracts.Dtos;

/// <summary>
/// PasswordBook DTO
/// </summary>
public class PasswordBookDto
{
    public required Guid Id { get; set; }
    public required Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PasswordType AllowedType { get; set; } = PasswordType.General;
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 20;
    public required DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public bool IsDeleted { get; set; }
    public int EntryCount { get; set; }
}

/// <summary>
/// Create or Update PasswordBook DTO
/// </summary>
public class CreateUpdatePasswordBookDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int MinLength { get; set; } = 8;
    public int MaxLength { get; set; } = 20;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecialChar { get; set; } = true;
    public string? SpecialChars { get; set; }
    public PasswordType AllowedType { get; set; } = PasswordType.General;
}

/// <summary>
/// PasswordEntry DTO
/// </summary>
public class PasswordEntryDto
{
    public Guid Id { get; set; }
    public Guid PasswordBookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool HasUsername { get; set; }
    public string? Username { get; set; }
    public PasswordType PasswordType { get; set; }
    public PasswordWeakLevel? WeakLevel { get; set; }
    public string? Remark { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Create PasswordEntry DTO
/// </summary>
public class CreatePasswordEntryDto
{
    public string Title { get; set; } = string.Empty;
    public bool HasUsername { get; set; }
    public string? Username { get; set; }
    public PasswordType PasswordType { get; set; }
    public PasswordWeakLevel? WeakLevel { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? Remark { get; set; }
}

/// <summary>
/// Update Password DTO
/// </summary>
public class UpdatePasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Generate Random Password Input DTO
/// </summary>
public class GenerateRandomPasswordDto
{
    public Guid PasswordBookId { get; set; }
    public int Length { get; set; } = 12;
    public PasswordCharacterType CharacterTypes { get; set; } = PasswordCharacterType.All;
    public bool IsOnlyOnce { get; set; } = true;
    public string? IgnoreChars { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
}

/// <summary>
/// Generate Random Password Result DTO
/// </summary>
public class GenerateRandomPasswordResultDto
{
    public string Password { get; set; } = string.Empty;
    public PasswordWeakLevel WeakLevel { get; set; }
}
