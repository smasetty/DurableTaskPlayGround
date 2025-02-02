using Common.Logging;
using DurableTask.AzureStorage;
using DurableTask.Core;
using DurableTask.Core.Tracing;
using DurableTaskSamples.Activities;
using DurableTaskSamples.Orchestrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.DependencyInjection;

namespace DurableTaskWorker;

/// <summary>
/// Represents a worker that manages the lifecycle of a Durable Task Hub.
/// </summary>
internal class DurableTaskWorker
{
    /// <summary>
    /// The configuration settings for the worker.
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// The logger instance for the worker.
    /// </summary>
    private readonly ILoggerService _logger;

    /// <summary>
    /// The Azure Storage orchestration service that manages task persistence and communication.
    /// </summary>
    private readonly AzureStorageOrchestrationService _orchestrationService;

    /// <summary>
    /// The task hub worker instance that processes orchestrations and activities.
    /// Can be null before StartAsync is called or after StopAsync is called.
    /// </summary>
    private TaskHubWorker? _taskHubWorker;

    /// <summary>
    /// The service provider for dependency injection.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DurableTaskWorker"/> class.
    /// </summary>
    /// <param name="configuration">The configuration settings.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="orchestrationService">The Azure Storage Orchestration Service instance.</param>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public DurableTaskWorker(
        IConfiguration configuration,
        ILoggerService logger,
        AzureStorageOrchestrationService orchestrationService,
        IServiceProvider serviceProvider)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _orchestrationService = orchestrationService ?? throw new ArgumentNullException(nameof(orchestrationService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Starts the Task Hub Worker asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync()
    {
        if (_configuration.GetValue<bool>("ShouldLogVerbose"))
        {
            var eventListener = new ObservableEventListener();
            eventListener.LogToConsole(formatter: new DtfEventFormatter());
            eventListener.EnableEvents(DefaultEventSource.Log, EventLevel.Informational);
        }

        _taskHubWorker = new TaskHubWorker(_orchestrationService);

        // Register orchestrations and activities with DI support
        _taskHubWorker.AddTaskOrchestrations(new DependencyInjectionObjectCreator<TaskOrchestration>(_serviceProvider, typeof(SimpleOrchestration)));
        // _taskHubWorker.AddTaskOrchestrations(new DependencyInjectionObjectCreator<TaskOrchestration>(_serviceProvider, typeof(ComplexOrchestration)));

        _taskHubWorker.AddTaskActivities(new DependencyInjectionObjectCreator<TaskActivity>(_serviceProvider, typeof(SimpleGreetingActivity)));
        // _taskHubWorker.AddTaskActivities(new DependencyInjectionObjectCreator<TaskActivity>(_serviceProvider, typeof(SumActivity)));

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
/// Creates objects using dependency injection or falls back to default creation
/// </summary>
/// <typeparam name="T">The type of object to create</typeparam>
/// <summary>
/// Creates objects using dependency injection or falls back to default creation.
/// This class extends DefaultObjectCreator to provide DI support for creating task orchestrations and activities.
/// </summary>
/// <typeparam name="T">The type of object to create. Must be a class.</typeparam>
public class DependencyInjectionObjectCreator<T> : DefaultObjectCreator<T> where T : class
{
    /// <summary>
    /// The service provider used for dependency injection.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// The type of object to create.
    /// </summary>
    private readonly Type _type;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyInjectionObjectCreator{T}"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="type">The type of object to create.</param>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider or type is null.</exception>
    public DependencyInjectionObjectCreator(IServiceProvider serviceProvider, Type type)
        :base(type)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <summary>
    /// Creates an instance of the specified type using dependency injection.
    /// </summary>
    /// <returns>An instance of type T.</returns>
    /// <exception cref="InvalidOperationException">Thrown when object creation fails.</exception>
    public override T Create()
    {
        return ActivatorUtilities.GetServiceOrCreateInstance(_serviceProvider, _type) as T 
            ?? throw new InvalidOperationException($"Failed to create instance of type {_type.Name}");
    }
}