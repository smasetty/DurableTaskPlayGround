using DurableTask.AzureStorage;
using DurableTask.Core;
using DurableTaskSamples.Activities;
using DurableTaskSamples.Orchestrations;
using Microsoft.Extensions.Logging;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace DurableTaskWorker;
internal class DurableTaskWorker 
{
    private TaskHubWorker? taskHubWorker;

    /// <summary>
    /// Gets the Azure Storage Orchestration Service handle.
    /// </summary>
    /// <returns>An instance of <see cref="AzureStorageOrchestrationService"/>.</returns>
    public static AzureStorageOrchestrationService GetAzureStorageOrchestrationServiceHandle()
    {
        var storageConnectionString = ConfigurationManager.AppSettings["AzureStorageConnectionString"];
        if (string.IsNullOrEmpty(storageConnectionString))
        {
            Console.WriteLine($"Azure storage connection string is empty");
            Environment.Exit(0);
        }

        var taskHubName = ConfigurationManager.AppSettings["TaskHubName"];
        if (string.IsNullOrEmpty(taskHubName))
        {
            Console.WriteLine($"Task Hub name is empty");
            Environment.Exit(0);
        }

        var azureStorageSettings = new AzureStorageOrchestrationServiceSettings
        {
            StorageAccountClientProvider = new StorageAccountClientProvider(storageConnectionString),
            TaskHubName = taskHubName,
        };

        var shouldLogAzureStorageEvents = bool.Parse(ConfigurationManager.AppSettings["LogAzureStorageTraces"] ?? string.Empty);
        if (shouldLogAzureStorageEvents)
        {
            LoggerFactory.Create(b => b.AddConsole());
        }

        var orchestrationServiceAndClient = new AzureStorageOrchestrationService(azureStorageSettings);

        return orchestrationServiceAndClient;
    }

    /// <summary>
    /// Starts the Task Hub Worker asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync()
    {
        taskHubWorker = new TaskHubWorker(GetAzureStorageOrchestrationServiceHandle());

        taskHubWorker.AddTaskOrchestrations(typeof(SimpleOrchestration))
            .AddTaskActivities(typeof(SimpleGreetingActivity));

        await taskHubWorker.StartAsync();
    }

    /// <summary>
    /// Stops the Task Hub Worker asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public void Stop()
    {
        taskHubWorker?.Dispose();
    }
}
