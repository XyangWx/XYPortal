using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using XYPortal.RandomStringProvider.RandomStringProvider;

namespace XYPortal.RandomStringProvider;

/// <summary>
/// 测试六十四卦 (64 Hexagrams) 相关的 RandomStringProvider 功能
/// </summary>
public class SixtyFourHexagrams_Tests : RandomStringProviderApplicationTestBase<RandomStringProviderApplicationTestModule>
{
    private readonly RandomStringApplication _randomStringApp;

    public SixtyFourHexagrams_Tests()
    {
        _randomStringApp = GetRequiredService<RandomStringApplication>();
    }

    [Fact]
    public void MakeRandomString_WithSixtyFourHexagrams_ShouldReturnHexagramCharacters()
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = 10,
            SymbolCategories = RandomCategory.SixtyFourHexagrams
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(10);
        
        // Verify all characters are hexagrams (U+4DC0 to U+4DFF)
        foreach (char c in result)
        {
            var codePoint = (int)c;
            codePoint.ShouldBeInRange(0x4DC0, 0x4DFF, 
                $"Character '{c}' (U+{codePoint:X4}) is not a hexagram");
        }
    }

    [Fact]
    public void MakeRandomString_WithSixtyFourHexagrams_ShouldReturnUniqueCharacters()
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = 50,
            SymbolCategories = RandomCategory.SixtyFourHexagrams,
            IsOnlyOnce = true
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(50);
        
        // With IsOnlyOnce, all characters should be unique
        result.Distinct().Count().ShouldBe(50);
    }

    [Fact]
    public void MakeRandomString_WithSixtyFourHexagrams_CombinedWithOtherCategories_ShouldOnlyContainHexagrams()
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = 5,
            SymbolCategories = RandomCategory.SixtyFourHexagrams | RandomCategory.ArabicNumerals
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(5);
        
        // All characters should be hexagrams (not Arabic numerals)
        foreach (char c in result)
        {
            var codePoint = (int)c;
            // Either it's a hexagram or an Arabic numeral (0-9)
            var isHexagram = codePoint >= 0x4DC0 && codePoint <= 0x4DFF;
            var isArabicNumeral = codePoint >= '0' && codePoint <= '9';
            (isHexagram || isArabicNumeral).ShouldBeTrue();
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(32)]
    [InlineData(64)]
    public void MakeRandomString_WithSixtyFourHexagrams_VariousLengths_ShouldReturnCorrectLength(int length)
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = length,
            SymbolCategories = RandomCategory.SixtyFourHexagrams
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(length);
    }

    [Fact]
    public async Task MakeRandomStringAsync_WithSixtyFourHexagrams_ShouldWork()
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = 8,
            SymbolCategories = RandomCategory.SixtyFourHexagrams
        };

        // Act
        var result = await _randomStringApp.MakeRandomStringAsync(input);

        // Assert
        result.Length.ShouldBe(8);
        
        foreach (char c in result)
        {
            var codePoint = (int)c;
            codePoint.ShouldBeInRange(0x4DC0, 0x4DFF);
        }
    }
}
