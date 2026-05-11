using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using XYPortal.RandomStringProvider.RandomStringProvider;

namespace XYPortal.RandomStringProvider;

/// <summary>
/// 测试 Unicode Miscellaneous Symbols 相关的 RandomStringProvider 功能
/// </summary>
public class UnicodeMiscSymbols_Tests : RandomStringProviderApplicationTestBase<RandomStringProviderApplicationTestModule>
{
    private readonly RandomStringApplication _randomStringApp;

    public UnicodeMiscSymbols_Tests()
    {
        _randomStringApp = GetRequiredService<RandomStringApplication>();
    }

    [Fact]
    public void MakeRandomString_WithUnicodeMiscSymbols_ShouldReturnMiscSymbolCharacters()
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = 10,
            SymbolCategories = RandomCategory.UnicodeMiscellaneousSymbols
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(10);
        
        // Verify all characters are Unicode miscellaneous symbols (U+2600 to U+26FF, U+2638 to U+27BF)
        foreach (char c in result)
        {
            var codePoint = (int)c;
            var isMiscSymbol = (codePoint >= 0x2600 && codePoint <= 0x26FF) || 
                              (codePoint >= 0x2638 && codePoint <= 0x27BF);
            isMiscSymbol.ShouldBeTrue(
                $"Character '{c}' (U+{codePoint:X4}) is not a Unicode miscellaneous symbol");
        }
    }

    [Fact]
    public void MakeRandomString_WithUnicodeMiscSymbols_ShouldReturnUniqueCharacters()
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = 50,
            SymbolCategories = RandomCategory.UnicodeMiscellaneousSymbols,
            IsOnlyOnce = true
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(50);
        
        // With IsOnlyOnce, all characters should be unique
        result.Distinct().Count().ShouldBe(50);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(20)]
    [InlineData(50)]
    public void MakeRandomString_WithUnicodeMiscSymbols_VariousLengths_ShouldReturnCorrectLength(int length)
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = length,
            SymbolCategories = RandomCategory.UnicodeMiscellaneousSymbols
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(length);
    }

    [Fact]
    public void MakeRandomString_WithMultipleCategories_IncludingUnicodeMisc_ShouldWork()
    {
        // Arrange - symbolCategories = 527 = Lowercase(1) + Uppercase(2) + Numbers(4) + Punctuation(8) + Chinese(16) + Heavenly(32) + Earthly(64) + Bagua(128) + Hexagrams(256) + UnicodeMisc(512)
        var input = new RandomStringInput
        {
            Length = 20,
            SymbolCategories = (RandomCategory)527
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(20);
    }

    [Fact]
    public async Task MakeRandomStringAsync_WithUnicodeMiscSymbols_ShouldWork()
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = 8,
            SymbolCategories = RandomCategory.UnicodeMiscellaneousSymbols
        };

        // Act
        var result = await _randomStringApp.MakeRandomStringAsync(input);

        // Assert
        result.Length.ShouldBe(8);
        
        foreach (char c in result)
        {
            var codePoint = (int)c;
            var isMiscSymbol = (codePoint >= 0x2600 && codePoint <= 0x26FF) || 
                              (codePoint >= 0x2638 && codePoint <= 0x27BF);
            isMiscSymbol.ShouldBeTrue();
        }
    }

    [Fact]
    public void MakeRandomString_WithAllCategories_ShouldWork()
    {
        // Arrange - All categories combined
        var input = new RandomStringInput
        {
            Length = 30,
            SymbolCategories = RandomCategory.All
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(30);
    }

    [Fact]
    public void MakeRandomString_UnicodeTest()
    {
        var input = new RandomStringInput
        {
            Length = 18,
            SymbolCategories = RandomCategory.LowercaseLetters | RandomCategory.UppercaseLetters | RandomCategory.ArabicNumerals | RandomCategory.EnglishPunctuation | RandomCategory.UnicodeMiscellaneousSymbols
        };
        
        var result = _randomStringApp.MakeRandomString(input);
        
        result.Length.ShouldBe(18);
    }
}
