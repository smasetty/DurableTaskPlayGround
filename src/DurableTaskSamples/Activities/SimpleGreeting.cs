using Common.Logging;
using DurableTask.Core;

namespace DurableTaskSamples.Activities;

/// <summary>
/// Represents an activity that performs a simple greeting task.
/// </summary>
public class SimpleGreetingActivity(Logger logger) : AsyncTaskActivity<int, bool>
{
    private readonly Logger _logger = logger;
    private const string Source = "SimpleGreetingActivity";

    /// <summary>
    /// Executes the activity asynchronously.
    /// </summary>
    /// <param name="context">The task context.</param>
    /// <param name="input">The input value for the activity.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a boolean value indicating the success of the activity.</returns>
    protected override async Task<bool> ExecuteAsync(TaskContext context, int input)
    {
        _logger.LogVerbose(Source, "Starting");

        await Task.Delay(5).ConfigureAwait(false);
        _logger.Log(Source, $"Executing {input}");

        await Task.Delay(2000).ConfigureAwait(false);
        _logger.LogVerbose(Source, "Completed");

        // If the input is greater than or equal to 2,
        // the activity is considered successful.
        if (input >= 2) return true;

        _logger.Log(Source, "Invalid input");
        return false;
    }
}
