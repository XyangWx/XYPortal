namespace XYPortal.RandomStringProvider.RandomStringProvider;

public class RandomStringInput
{
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public int Length { get; set; } = 12;
    public RandomCategory SymbolCategories { get; set; } = RandomCategory.LowercaseLetters |
                                                           RandomCategory.UppercaseLetters |
                                                           RandomCategory.ArabicNumerals |
                                                           RandomCategory.EnglishPunctuation;
}