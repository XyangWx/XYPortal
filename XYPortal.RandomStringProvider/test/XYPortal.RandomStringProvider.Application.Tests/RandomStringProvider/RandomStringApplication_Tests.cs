using System.Threading.Tasks;
using Shouldly;
using Xunit;
using XYPortal.RandomStringProvider.RandomStringProvider;

namespace XYPortal.RandomStringProvider;

/// <summary>
/// 测试 RandomStringApplication 输入长度与输出字符串长度的关系
/// </summary>
public class RandomStringApplication_Tests : RandomStringProviderApplicationTestBase<RandomStringProviderApplicationTestModule>
{
    private readonly RandomStringApplication _randomStringApp;

    public RandomStringApplication_Tests()
    {
        _randomStringApp = GetRequiredService<RandomStringApplication>();
    }

    [Fact]
    public void MakeRandomString_Should_Return_String_With_Correct_Length_When_No_Prefix_And_Suffix()
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = 16,
            SymbolCategories = RandomCategory.LowercaseLetters | RandomCategory.ArabicNumerals
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(input.Length);
    }

    [Fact]
    public void MakeRandomString_Should_Return_String_With_Correct_Length_When_Has_Prefix()
    {
        // Arrange
        var prefix = "PRE_";
        var input = new RandomStringInput
        {
            Prefix = prefix,
            Length = 12,
            SymbolCategories = RandomCategory.LowercaseLetters | RandomCategory.ArabicNumerals
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(prefix.Length + input.Length);
    }

    [Fact]
    public void MakeRandomString_Should_Return_String_With_Correct_Length_When_Has_Suffix()
    {
        // Arrange
        var suffix = "_SUF";
        var input = new RandomStringInput
        {
            Suffix = suffix,
            Length = 8,
            SymbolCategories = RandomCategory.LowercaseLetters | RandomCategory.ArabicNumerals
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(suffix.Length + input.Length);
    }

    [Fact]
    public void MakeRandomString_Should_Return_String_With_Correct_Length_When_Has_Both_Prefix_And_Suffix()
    {
        // Arrange
        var prefix = "START_";
        var suffix = "_END";
        var input = new RandomStringInput
        {
            Prefix = prefix,
            Suffix = suffix,
            Length = 20,
            SymbolCategories = RandomCategory.LowercaseLetters | RandomCategory.UppercaseLetters | RandomCategory.ArabicNumerals
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(prefix.Length + input.Length + suffix.Length);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void MakeRandomString_Should_Return_String_With_Specified_Length(int length)
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = length,
            SymbolCategories = RandomCategory.ArabicNumerals
        };

        // Act
        var result = _randomStringApp.MakeRandomString(input);

        // Assert
        result.Length.ShouldBe(length);
    }

    [Fact]
    public async Task MakeRandomStringAsync_Should_Return_String_With_Correct_Length()
    {
        // Arrange
        var input = new RandomStringInput
        {
            Length = 15,
            Prefix = "ID_",
            Suffix = "_V1",
            SymbolCategories = RandomCategory.LowercaseLetters | RandomCategory.ArabicNumerals
        };

        // Act
        var result = await _randomStringApp.MakeRandomStringAsync(input);

        // Assert
        result.Length.ShouldBe(input.Prefix!.Length + input.Length + input.Suffix!.Length);
    }
}
