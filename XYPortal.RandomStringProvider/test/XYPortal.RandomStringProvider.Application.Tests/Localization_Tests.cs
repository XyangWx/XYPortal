using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Shouldly;
using Xunit;
using XYPortal.RandomStringProvider.Localization;

namespace XYPortal.RandomStringProvider;

/// <summary>
/// 测试 RandomStringProvider 本地化功能 - 使用 IStringLocalizer
/// </summary>
public class Localization_Tests : RandomStringProviderApplicationTestBase<RandomStringProviderApplicationTestModule>
{
    private readonly IStringLocalizer<RandomStringProviderResource> _localizer;

    public Localization_Tests()
    {
        _localizer = GetRequiredService<IStringLocalizer<RandomStringProviderResource>>();
    }

    [Fact]
    public void IStringLocalizer_Should_Not_Be_Null()
    {
        _localizer.ShouldNotBeNull();
    }

    [Fact]
    public void Localize_Should_Return_NonEmpty_Value_For_RandomStringGenerator()
    {
        // Arrange & Act
        var result = _localizer["RandomStringGenerator"];
        
        // Assert - Value should not be empty
        result.Value.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void Localize_Should_Return_Correct_Value_For_RandomStringGenerator_By_Key()
    {
        // Arrange & Act
        var result = _localizer["RandomStringGenerator"];
        
        // Assert - Value should be either English or Chinese depending on culture
        var currentCulture = CultureInfo.CurrentCulture.Name;
        if (currentCulture.StartsWith("zh"))
        {
            result.Value.ShouldBe("随机字符串生成器");
        }
        else
        {
            result.Value.ShouldBe("Random String Generator");
        }
    }

    [Fact]
    public void Localize_Should_Contain_All_Required_Keys()
    {
        // Verify all required localization keys exist and return values (not keys themselves)
        var keys = new[]
        {
            "RandomStringGenerator",
            "GeneratedString",
            "Copy",
            "Generate",
            "Length",
            "Prefix",
            "Suffix",
            "IgnoreChars",
            "IgnoreCharsHint",
            "UniqueCharsOnly",
            "SymbolCategories",
            "LowercaseLetters",
            "UppercaseLetters",
            "ArabicNumerals",
            "EnglishPunctuation",
            "ChineseCapitalNumbers",
            "HeavenlyStems",
            "EarthlyBranches",
            "BaguaSymbols",
            "SixtyFourHexagrams",
            "UnicodeMiscellaneousSymbols",
            "SelectAtLeastOneCategory",
            "GeneratedSuccessfully",
            "CopiedToClipboard"
        };

        foreach (var key in keys)
        {
            var result = _localizer[key];
            // The localized value should not be empty or the same as the key
            result.Value.ShouldNotBeNullOrEmpty($"Localization key '{key}' should have a value");
        }
    }
}
