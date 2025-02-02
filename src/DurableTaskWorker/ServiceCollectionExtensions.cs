using Azure.Identity;
using Common.Logging;
using DurableTask.AzureStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DurableTaskWorker;

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
            var logger = provider.GetRequiredService<ILoggerService>();
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

            logger.Log(nameof(ServiceCollectionExtensions), 
                $"Configuration values: AzureStorageAccountName={storageAccountName},\nTaskHubName={taskHubName}");

            var credential = new DefaultAzureCredential();

            logger.Log(nameof(ServiceCollectionExtensions), 
                $"Configuration values: credential={credential}");

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
                azureStorageSettings.LoggerFactory = loggerFactory;
            }

            return new AzureStorageOrchestrationService(azureStorageSettings);
        });

        services.AddSingleton<DurableTaskWorker>();

        return services;
    }
} 