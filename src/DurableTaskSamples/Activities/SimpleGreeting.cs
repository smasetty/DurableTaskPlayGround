using Common.Logging;
using DurableTask.Core;

namespace DurableTaskSamples.Activities;

public class SimpleGreetingActivity : AsyncTaskActivity<int, bool>
{
    private const string Source = "SimpleGreetingActivity";

    protected override async Task<bool> ExecuteAsync(TaskContext context, int input)
    {
        Logger.Log(Source, "Starting");

        await Task.Delay(5).ConfigureAwait(false);
        Logger.Log(Source, $"Executing {input}");

        await Task.Delay(2000).ConfigureAwait(false);
        Logger.Log(Source, "Completed");

        if (input >= 2) return true;

        Logger.Log(Source, "Invalid input");
        return false;
    }
}