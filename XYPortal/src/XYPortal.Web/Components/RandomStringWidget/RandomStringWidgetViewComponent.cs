using Microsoft.AspNetCore.Mvc;
using XYPortal.RandomStringProvider.RandomStringProvider;

namespace XYPortal.Web.Components.RandomStringWidget;

public class RandomStringWidgetViewComponent : ViewComponent
{
    private readonly IRandomStringApplication _randomStringApplication;

    public RandomStringWidgetViewComponent(IRandomStringApplication randomStringApplication)
    {
        _randomStringApplication = randomStringApplication;
    }

    public IViewComponentResult Invoke(string? prefix = null, string? suffix = null, int length = 12)
    {
        var input = new RandomStringInput
        {
            Prefix = prefix,
            Suffix = suffix,
            Length = length
        };

        var result = _randomStringApplication.MakeRandomString(input);
        
        return View("Default", result);
    }
}
