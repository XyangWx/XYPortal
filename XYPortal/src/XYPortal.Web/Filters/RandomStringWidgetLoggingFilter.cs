using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace XYPortal.Web.Filters;

public class RandomStringWidgetLoggingFilter : IActionFilter
{
    private readonly ILogger<RandomStringWidgetLoggingFilter> _logger;

    public RandomStringWidgetLoggingFilter(ILogger<RandomStringWidgetLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var actionDescriptor = context.ActionDescriptor;
        _logger.LogInformation("=== RandomStringWidget Logging ===");
        _logger.LogInformation("Action Descriptor: {ActionDescriptor}", actionDescriptor.DisplayName);
        _logger.LogInformation("Controller: {Controller}", context.Controller);
        _logger.LogInformation("Action Arguments: {Arguments}", string.Join(", ", context.ActionDescriptor.Parameters.Select(p => $"{p.Name}:{p.ParameterType.Name}")));
        
        if (context.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerAction)
        {
            _logger.LogInformation("Controller Name: {ControllerName}", controllerAction.ControllerName);
            _logger.LogInformation("Action Name: {ActionName}", controllerAction.ActionName);
        }

        foreach (var arg in context.ActionArguments)
        {
            _logger.LogInformation("Argument: {Key} = {Value}", arg.Key, arg.Value);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInformation("Action Executed. Result: {Result}", context.Result?.GetType().Name);
    }
}
