using System;
using System.Text.RegularExpressions;
using XYPortal.PasswordBook.Enums;

namespace XYPortal.PasswordBook.ValueObjects;

/// <summary>
/// Password Format Requirement Value Object
/// </summary>
public class PasswordFormatRequirement
{
    public int MinLength { get; private set; }
    public int MaxLength { get; private set; }
    public bool RequireUppercase { get; private set; }
    public bool RequireLowercase { get; private set; }
    public bool RequireDigit { get; private set; }
    public bool RequireSpecialChar { get; private set; }
    public string SpecialChars { get; private set; }
    public PasswordType AllowedType { get; private set; }

    private PasswordFormatRequirement() 
    {
        SpecialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";
    }

    public PasswordFormatRequirement(
        int minLength,
        int maxLength,
        bool requireUppercase = false,
        bool requireLowercase = false,
        bool requireDigit = false,
        bool requireSpecialChar = false,
        string? specialChars = null,
        PasswordType? allowedType = null)
    {
        if (minLength < 1)
            throw new ArgumentException("Min length cannot be less than 1", nameof(minLength));
        if (maxLength < minLength)
            throw new ArgumentException("Max length cannot be less than min length", nameof(maxLength));

        MinLength = minLength;
        MaxLength = maxLength;
        RequireUppercase = requireUppercase;
        RequireLowercase = requireLowercase;
        RequireDigit = requireDigit;
        RequireSpecialChar = requireSpecialChar;
        SpecialChars = specialChars ?? "!@#$%^&*()-_=+[]{}|;:,.<>?";
        AllowedType = allowedType ?? PasswordType.General;
    }

    /// <summary>
    /// Validate if password meets format requirements
    /// </summary>
    public (bool IsValid, string? ErrorMessage) Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "Password cannot be empty");

        if (password.Length < MinLength)
            return (false, $"Password length cannot be less than {MinLength} characters");

        if (password.Length > MaxLength)
            return (false, $"Password length cannot exceed {MaxLength} characters");

        if (RequireUppercase && !Regex.IsMatch(password, "[A-Z]"))
            return (false, "Password must contain uppercase letters");

        if (RequireLowercase && !Regex.IsMatch(password, "[a-z]"))
            return (false, "Password must contain lowercase letters");

        if (RequireDigit && !Regex.IsMatch(password, "[0-9]"))
            return (false, "Password must contain digits");

        if (RequireSpecialChar && !Regex.IsMatch(password, $"[{Regex.Escape(SpecialChars)}]"))
            return (false, $"Password must contain special characters: {SpecialChars}");

        return (true, null);
    }

    /// <summary>
    /// Default numeric password format (6 digits)
    /// </summary>
    public static PasswordFormatRequirement DefaultNumeric => new(
        minLength: 6,
        maxLength: 6,
        requireDigit: true
    );

    /// <summary>
    /// Default general password format (8-20 characters, with uppercase, lowercase and digits)
    /// </summary>
    public static PasswordFormatRequirement DefaultGeneral => new(
        minLength: 8,
        maxLength: 20,
        requireUppercase: true,
        requireLowercase: true,
        requireDigit: true,
        requireSpecialChar: true
    );
}
