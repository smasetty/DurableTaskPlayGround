namespace Common.Logging;

/// <summary>
/// Provides logging functionality for the application.
/// </summary>
public static class Logger
{
    private static bool _shouldLogVerbose = true;

    /// <summary>
    /// Logs a message with the specified source.
    /// </summary>
    /// <param name="source">The source of the log message.</param>
    /// <param name="message">The log message.</param>
    public static void Log(string source, string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"[{0}] [{source}] {message}",
                    DateTime.UtcNow.TimeOfDay.ToString("c"));
        Console.ResetColor();
    }

    /// <summary>
    /// Logs a verbose message with the specified source if verbosity is enabled.
    /// </summary>
    /// <param name="source">The source of the log message.</param>
    /// <param name="message">The log message.</param>
    public static void LogVerbose(string source, string message)
    {
        if (!_shouldLogVerbose) return;
        Log(source, message);
    }

    /// <summary>
    /// Sets the verbosity level for logging.
    /// </summary>
    /// <param name="verbosity">If true, verbose logging is enabled; otherwise, it is disabled.</param>
    public static void SetVerbosity(bool verbosity)
    {
        _shouldLogVerbose = verbosity;
    }
}
