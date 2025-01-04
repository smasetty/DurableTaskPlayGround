using Common.Logging;
using DurableTask.Core;
using DurableTaskSamples.Activities;

namespace DurableTaskSamples.Orchestrations;

/// <summary>
/// Represents a simple orchestration task that processes an integer input and returns a boolean result.
/// </summary>
public class SimpleOrchestration : TaskOrchestration<bool, int>
{
    private const string Source = "SimpleOrchestration";

    /// <summary>
    /// Runs the orchestration task.
    /// </summary>
    /// <param name="context">The orchestration context.</param>
    /// <param name="input">The input value for the task.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a boolean indicating success or failure.</returns>
    public override async Task<bool> RunTask(OrchestrationContext context, int input)
    {
        try
        {
            Logger.Log(Source, $"Initiating, IsReplaying: {context.IsReplaying}");
            await context.ScheduleTask<bool>(typeof(SimpleGreetingActivity), input);
            await context.ScheduleTask<bool>(typeof(SimpleGreetingActivity), input);
            await context.ScheduleTask<bool>(typeof(SimpleGreetingActivity), 555);
            Logger.Log(Source, "Completed");

            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(Source, ex.ToString());
            return false;
        }
    }
}
