using Common.Logging;
using DurableTask.Core;

namespace DurableTaskSamples.Activities;

/// <summary>
/// Represents an activity that performs a simple greeting task.
/// </summary>
public sealed class SimpleGreetingActivity : AsyncTaskActivity<int, bool>
{
    private readonly ILoggerService _logger;
    private const string Source = nameof(SimpleGreetingActivity);
    private const int MinimumValidInput = 2;
    private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleGreetingActivity"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public SimpleGreetingActivity(ILoggerService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes the activity asynchronously.
    /// </summary>
    /// <param name="context">The task context.</param>
    /// <param name="input">The input value for the activity.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a boolean value indicating the success of the activity.</returns>
    protected override async Task<bool> ExecuteAsync(TaskContext context, int input)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            _logger.LogVerbose(Source, "Starting");

            await Task.Delay(ProcessingDelay);
            _logger.Log(Source, $"Executing {input}");

            if (input < MinimumValidInput)
            {
                _logger.Log(Source, "Invalid input");
                return false;
            }

            _logger.LogVerbose(Source, "Completed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(Source, $"Error processing input: {ex}");
            throw;
        }
    }
}

