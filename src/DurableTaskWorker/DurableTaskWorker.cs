using Azure.Core;
using Azure.Identity;
using DurableTask.AzureStorage;
using DurableTask.Core;
using DurableTaskSamples.Activities;
using DurableTaskSamples.Orchestrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DurableTaskWorker;

/// <summary>
/// Represents a worker that manages the lifecycle of a Durable Task Hub.
/// </summary>
internal class DurableTaskWorker
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DurableTaskWorker> _logger;
    private readonly AzureStorageOrchestrationService _orchestrationService;
    private TaskHubWorker? _taskHubWorker;

    /// <summary>
    /// Initializes a new instance of the <see cref="DurableTaskWorker"/> class.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="orchestrationService">The Azure Storage Orchestration Service instance.</param>
    public DurableTaskWorker(
        IConfiguration configuration,
        ILogger<DurableTaskWorker> logger,
        AzureStorageOrchestrationService orchestrationService)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orchestrationService = orchestrationService ?? throw new ArgumentNullException(nameof(orchestrationService));
    }

    /// <summary>
    /// Starts the Task Hub Worker asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync()
    {
        _taskHubWorker = new TaskHubWorker(_orchestrationService);
        _taskHubWorker.AddTaskOrchestrations(typeof(SimpleOrchestration))
            .AddTaskActivities(typeof(SimpleGreetingActivity));
        await _taskHubWorker.StartAsync();
    }

    /// <summary>
    /// Stops the Task Hub Worker asynchronously.
    /// </summary>
    public async Task StopAsync()
    {
        if (_taskHubWorker != null)
        {
            await _taskHubWorker.StopAsync();
            _taskHubWorker.Dispose();
        }
    }
}

/// <summary>
/// Provides extension methods for adding Durable Task Worker services to an IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Durable Task Worker services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns>The IServiceCollection with the Durable Task Worker services added.</returns>
    public static IServiceCollection AddDurableTaskWorker(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILogger<DurableTaskWorker>>();
            var storageAccountName = configuration["StorageAccountName"];
            var taskHubName = configuration["TaskHubName"];

            if (string.IsNullOrEmpty(storageAccountName))
            {
                throw new ArgumentNullException(
                    nameof(storageAccountName),
                    "Azure Storage connection string is not configured.");
            }

            if (string.IsNullOrEmpty(taskHubName))
            {
                throw new ArgumentNullException(
                    nameof(taskHubName),
                    "Task Hub name is not configured.");
            }

            logger.LogInformation($"Configuration values: AzureStorageAccountName={storageAccountName},\nTaskHubName={taskHubName}");

            var credential = new DefaultAzureCredential();

            //             // Define the Azure resource scope you want the token for
            // var tokenRequestContext = new TokenRequestContext(new[] { "https://storage.azure.com/.default" });
            //
            // // Attempt to acquire a token
            // var token = credential.GetToken(tokenRequestContext);
            //
            // // If successful, display token details
            // Console.WriteLine("Token acquired successfully!");
            // Console.WriteLine($"Token: {token.Token}");
            // Console.WriteLine($"Expires On: {token.ExpiresOn}");

            logger.LogInformation($"Configuration values: credential={credential}");

            var azureStorageSettings = new AzureStorageOrchestrationServiceSettings
            {
                StorageAccountClientProvider = new StorageAccountClientProvider(storageAccountName, credential),
                TaskHubName = taskHubName,
            };

            if (bool.TryParse(configuration["LogAzureStorageTraces"], out var shouldLogAzureStorageEvents)
                && shouldLogAzureStorageEvents)
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                loggerFactory.CreateLogger<DurableTaskWorker>();
            }

            return new AzureStorageOrchestrationService(azureStorageSettings);
        });

        services.AddSingleton<DurableTaskWorker>();

        return services;
    }
}
