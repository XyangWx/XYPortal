using Microsoft.Extensions.Logging;

namespace XYPortal.PasswordBook;

internal static class LoggerHelper
{
    private static ILoggerFactory? _loggerFactory = null;

    public static ILoggerFactory? SetFactory(ILoggerFactory? factory)
    {
        _loggerFactory = factory;
        return _loggerFactory;
    }

    public static ILogger<T>? CreateLogger<T>()
    {
        return _loggerFactory?.CreateLogger<T>();
    }
}