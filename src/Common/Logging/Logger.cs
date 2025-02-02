using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Logging;

/// <summary>
/// Defines methods for logging messages.
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// Logs a message with the specified source.
    /// </summary>
    /// <param name="source">The source of the log message.</param>
    /// <param name="message">The log message.</param>
    void Log(string source, string message);

    /// <summary>
    /// Logs a verbose message with the specified source.
    /// </summary>
    /// <param name="source">The source of the log message.</param>
    /// <param name="message">The log message.</param>
    void LogVerbose(string source, string message);

    /// <summary>
    /// Logs an error message with the specified source.
    /// </summary>
    /// <param name="source">The source of the log message.</param>
    /// <param name="message">The log message.</param>
    void LogError(string source, string message);
}

/// <summary>
/// Provides logging functionality for the application.
/// </summary>
public class Logger: ILoggerService 
{
    private readonly ILogger<Logger> _logger;
    private readonly bool _shouldLogVerbose;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="shouldLogVerbose">Indicates whether verbose logging is enabled.</param>
    public Logger(ILogger<Logger> logger, bool shouldLogVerbose = true)
    {
        _logger = logger;
        _shouldLogVerbose = shouldLogVerbose;
    }

    /// <summary>
    /// Logs a message with the specified source.
    /// </summary>
    /// <param name="source">The source of the log message.</param>
    /// <param name="message">The log message.</param>
    public void Log(string source, string message)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        _logger.LogInformation("[{Time}] [{Source}] {Message}", DateTime.UtcNow.TimeOfDay.ToString("c"), source, message);
    }

    /// <summary>
    /// Logs a verbose message with the specified source if verbosity is enabled.
    /// </summary>
    /// <param name="source">The source of the log message.</param>
    /// <param name="message">The log message.</param>
    public void LogVerbose(string source, string message)
    {
        if (!_shouldLogVerbose) return;
        _logger.LogDebug("[{Time}] [{Source}] {Message}", DateTime.UtcNow.TimeOfDay.ToString("c"), source, message);
    }

    /// <summary>
    /// Logs an error message with the specified source.
    /// </summary>
    /// <param name="source">The source of the log message.</param>
    /// <param name="message">The log message.</param>
    public void LogError(string source, string message)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        _logger.LogError("[{Time}] [{Source}] {Message}", DateTime.UtcNow.TimeOfDay.ToString("c"), source, message);
    }
}

/// <summary>
/// Provides extension methods for adding custom logging services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds custom logging services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add the logging services to.</param>
    /// <returns>The service collection with the logging services added.</returns>
    public static IServiceCollection AddCustomLogging(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerService, Logger>(p =>
        {
            var logger = p.GetRequiredService<ILogger<Logger>>();
            return new Logger(logger, true);
        });

        return services;
    }
}
