using Common.Logging;
using DurableTask.Core;
using DurableTaskSamples.Activities;

namespace DurableTaskSamples.Orchestrations;

/// <summary>
/// Represents a complex orchestration task that processes a list of integers and returns the sum of the results.
/// </summary>
public sealed class ComplexOrchestration : TaskOrchestration<int, List<int>>
{
    private readonly ILoggerService _logger;
    private const string Source = nameof(ComplexOrchestration);

    public ComplexOrchestration(ILoggerService logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs the orchestration task.
    /// </summary>
    /// <param name="context">The orchestration context.</param>
    /// <param name="input">The input list of integers for the task.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the sum of the results from the activities.</returns>
    public override async Task<int> RunTask(OrchestrationContext context, List<int> input)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(input);

        try
        {
            _logger.Log(Source, $"Initiating, IsReplaying: {context.IsReplaying}");
            
            // Process items in parallel using LINQ
            var tasks = input.Select(i => context.ScheduleTask<int>(typeof(SumActivity), i));
            var results = await Task.WhenAll(tasks);
            
            var sum = results.Sum();
            _logger.Log(Source, $"Completed with sum: {sum}");

            return sum;
        }
        catch (Exception ex)
        {
            _logger.LogError(Source, $"Error in orchestration: {ex}");
            throw; // Better to let the orchestration framework handle errors
        }
    }
}
