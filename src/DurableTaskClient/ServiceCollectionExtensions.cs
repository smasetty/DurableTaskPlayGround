using Azure.Identity;
using DurableTask.AzureStorage;
using DurableTask.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DurableTaskClient;

/// <summary>
/// Contains extension methods for configuring Durable Task Framework client services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Durable Task Framework client services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration section containing Durable Task settings.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <remarks>
    /// This method configures a singleton <see cref="TaskHubClient"/> that connects to Azure Storage.
    /// It requires the following configuration settings:
    /// - StorageAccountName: The name of the Azure Storage account
    /// - TaskHubName: The name of the Durable Task Hub
    /// 
    /// The client uses Azure DefaultAzureCredential for authentication.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when required configuration values are missing.</exception>
    public static IServiceCollection AddDurableTaskClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<TaskHubClient>(sp =>
        {
            var storageAccountName = configuration["StorageAccountName"];
            var taskHubName = configuration["TaskHubName"];

            ArgumentException.ThrowIfNullOrEmpty(storageAccountName);
            ArgumentException.ThrowIfNullOrEmpty(taskHubName);

            var settings = new AzureStorageOrchestrationServiceSettings
            {
                StorageAccountClientProvider = new StorageAccountClientProvider(
                    storageAccountName,
                    new DefaultAzureCredential()),
                TaskHubName = taskHubName
            };

            var orchestrationService = new AzureStorageOrchestrationService(settings);
            return new TaskHubClient(orchestrationService);
        });

        return services;
    }
} 