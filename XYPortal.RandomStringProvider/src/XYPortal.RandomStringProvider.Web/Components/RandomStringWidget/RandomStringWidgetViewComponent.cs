using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XYPortal.RandomStringProvider.RandomStringProvider;
using Volo.Abp.AspNetCore.Mvc;

namespace XYPortal.RandomStringProvider.Web.Components.RandomStringProvider;

public class RandomStringWidgetViewComponent : AbpViewComponent
{
    private readonly IRandomStringApplication _randomStringApplication;
    private readonly ILogger<RandomStringWidgetViewComponent> _logger;

    public RandomStringWidgetViewComponent(
        IRandomStringApplication randomStringApplication,
        ILogger<RandomStringWidgetViewComponent> logger)
    {
        _randomStringApplication = randomStringApplication;
        _logger = logger;
    }

    public IViewComponentResult Invoke(string? prefix = null, string? suffix = null, int length = 12)
    {
        _logger.LogInformation("=== RandomStringWidgetViewComponent.Invoke START ===");
        _logger.LogInformation("prefix: {prefix}, suffix: {suffix}, length: {length}", prefix, suffix, length);

        var input = new RandomStringInput
        {
            Prefix = prefix,
            Suffix = suffix,
            Length = length
        };

        var result = _randomStringApplication.MakeRandomString(input);
        _logger.LogInformation("Generated result: {result}", result);
        
        return View("/Views/Shared/Components/RandomStringWidget/Default.cshtml",result);
    }
}
