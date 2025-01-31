using Common.Logging;
using DurableTask.Core;

namespace DurableTaskSamples.Activities;

/// <summary>
/// Represents an activity that processes an integer input and returns the input added to a constant value.
/// </summary>
public sealed class SumActivity(Logger logger) : AsyncTaskActivity<int, int>
{
    private readonly Logger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const string Source = nameof(SumActivity);
    private const int AddValue = 10;
    private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Executes the activity asynchronously.
    /// </summary>
    /// <param name="context">The task context.</param>
    /// <param name="input">The input value for the activity.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the input value added to a constant value.</returns>
    protected override async Task<int> ExecuteAsync(TaskContext context, int input)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            _logger.LogVerbose(Source, "Starting");

            // Simulate some work
            await Task.Delay(ProcessingDelay);
            _logger.Log(Source, $"Processing {input}");

            var result = input + AddValue;
            _logger.LogVerbose(Source, "Completed");

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.Log(Source, "Operation was canceled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.Log(Source, $"Error processing input: {ex}");
            throw;
        }
    }
}
