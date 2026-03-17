using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using XYPortal.RandomStringProvider.RandomStringProvider;
using Volo.Abp.AspNetCore.Mvc;
using XYPortal.RandomStringProvider.Localization;

namespace XYPortal.RandomStringProvider.Web.Components.RandomStringProvider;

public class RandomStringWidgetViewComponent : AbpViewComponent
{
    private readonly IRandomStringApplication _randomStringApplication;
    private readonly ILogger<RandomStringWidgetViewComponent> _logger;
    private readonly IStringLocalizer<RandomStringProviderResource> _localizer;

    public RandomStringWidgetViewComponent(
        IRandomStringApplication randomStringApplication,
        ILogger<RandomStringWidgetViewComponent> logger,
        IStringLocalizer<RandomStringProviderResource> localizer)
    {
        _randomStringApplication = randomStringApplication;
        _localizer = localizer;
        _logger = logger;
    }

    public IViewComponentResult Invoke(string? prefix = null, string? suffix = null, int length = 12)
    {
        _logger.LogInformation("=== RandomStringWidgetViewComponent.Invoke START ===");
        _logger.LogInformation("prefix: {prefix}, suffix: {suffix}, length: {length}", prefix, suffix, length);
        _logger.LogInformation("CurrentCulture: {Culture}", CultureInfo.CurrentCulture.Name);
        _logger.LogInformation("RandomStringGenerator = {Value}", _localizer["RandomStringGenerator"].Value);

        var input = new RandomStringInput
        {
            Prefix = prefix,
            Suffix = suffix,
            Length = length
        };

        var result = _randomStringApplication.MakeRandomString(input);
        _logger.LogInformation("Generated result: {result}", result);

        // Pass localized strings via ViewBag
        ViewBag.L = new Dictionary<string, string>
        {
            ["RandomStringGenerator"] = _localizer["RandomStringGenerator"].Value,
            ["GeneratedString"] = _localizer["GeneratedString"].Value,
            ["Copy"] = _localizer["Copy"].Value,
            ["Generate"] = _localizer["Generate"].Value,
            ["Length"] = _localizer["Length"].Value,
            ["Prefix"] = _localizer["Prefix"].Value,
            ["Suffix"] = _localizer["Suffix"].Value,
            ["UniqueCharsOnly"] = _localizer["UniqueCharsOnly"].Value,
            ["SymbolCategories"] = _localizer["SymbolCategories"].Value,
            ["LowercaseLetters"] = _localizer["LowercaseLetters"].Value,
            ["UppercaseLetters"] = _localizer["UppercaseLetters"].Value,
            ["ArabicNumerals"] = _localizer["ArabicNumerals"].Value,
            ["EnglishPunctuation"] = _localizer["EnglishPunctuation"].Value,
            ["ChineseCapitalNumbers"] = _localizer["ChineseCapitalNumbers"].Value,
            ["HeavenlyStems"] = _localizer["HeavenlyStems"].Value,
            ["EarthlyBranches"] = _localizer["EarthlyBranches"].Value,
            ["BaguaSymbols"] = _localizer["BaguaSymbols"].Value,
            ["SixtyFourHexagrams"] = _localizer["SixtyFourHexagrams"].Value,
            ["UnicodeMiscellaneousSymbols"] = _localizer["UnicodeMiscellaneousSymbols"].Value,
            ["SelectAtLeastOneCategory"] = _localizer["SelectAtLeastOneCategory"].Value,
            ["GeneratedSuccessfully"] = _localizer["GeneratedSuccessfully"].Value,
            ["CopiedToClipboard"] = _localizer["CopiedToClipboard"].Value
        };
        
        return View("/Views/Shared/Components/RandomStringWidget/Default.cshtml", result);
    }
}
