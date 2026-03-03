using System.Collections.Generic;

namespace XYPortal.RandomStringProvider.RandomStringProvider;

public class RandomStringInput
{
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public int Length { get; set; } = 12;
    public List<char>? IgnoreChars { get; set; } = null;
    public bool IsOnlyOnce { get; set; } = false;
    public RandomCategory SymbolCategories { get; set; } = RandomCategory.LowercaseLetters |
                                                           RandomCategory.UppercaseLetters |
                                                           RandomCategory.ArabicNumerals |
                                                           RandomCategory.EnglishPunctuation;
}