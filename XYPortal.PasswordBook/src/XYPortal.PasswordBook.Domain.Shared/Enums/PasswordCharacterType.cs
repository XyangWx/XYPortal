using System;

namespace XYPortal.PasswordBook.Enums;

/// <summary>
/// Password Character Type
/// </summary>
[Flags]
public enum PasswordCharacterType : long
{
    /// <summary>
    /// Lowercase Letters
    /// </summary>
    LowercaseLetters = 1 << 0,

    /// <summary>
    /// Uppercase Letters
    /// </summary>
    UppercaseLetters = 1 << 1,

    /// <summary>
    /// Arabic Numerals (0-9)
    /// </summary>
    ArabicNumerals = 1 << 2,

    /// <summary>
    /// English Punctuation
    /// </summary>
    EnglishPunctuation = 1 << 3,

    /// <summary>
    /// All Types (Combination of all above)
    /// </summary>
    All = LowercaseLetters | UppercaseLetters | ArabicNumerals | EnglishPunctuation
}
