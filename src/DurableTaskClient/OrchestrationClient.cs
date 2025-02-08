using DurableTask.Core;
using DurableTaskSamples.Orchestrations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DurableTaskClient;

/// <summary>
/// Client service for managing and executing Durable Task orchestrations.
/// Provides a console-based interface for running different types of orchestrations.
/// </summary>
public sealed class OrchestrationClient(
    ILogger<OrchestrationClient> logger,
    TaskHubClient taskHubClient)
    : BackgroundService
{
    private readonly ILogger<OrchestrationClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly TaskHubClient _taskHubClient = taskHubClient ?? throw new ArgumentNullException(nameof(taskHubClient));

    /// <summary>
    /// Executes the background service, providing a console interface for running orchestrations.
    /// Continuously prompts the user to select and run different types of orchestrations until cancelled.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the service.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var orchestrations = new Dictionary<int, Type>
                {
                    { 1, typeof(SimpleOrchestration) },
                    { 2, typeof(ComplexOrchestration) }
                };

                Console.WriteLine("\nSelect an orchestration to run:");
                foreach (var kvp in orchestrations)
                {
                    Console.WriteLine($"{kvp.Key}. {kvp.Value.Name}");
                }
                Console.WriteLine("3. Exit");
                Console.Write("\nEnter your choice (1-3): ");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        await RunSimpleOrchestrationAsync(stoppingToken);
                        break;
                    case 2:
                        await RunComplexOrchestrationAsync(stoppingToken);
                        break;
                    case 3:
                        Environment.Exit(0);
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please select 1-3.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing orchestration");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Runs a simple orchestration with a single integer input value.
    /// Prompts the user for input and waits for the orchestration to complete.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the operation.</param>
    private async Task RunSimpleOrchestrationAsync(CancellationToken stoppingToken)
    {
        Console.Write("Enter an integer input value: ");
        if (!int.TryParse(Console.ReadLine(), out int input))
        {
            Console.WriteLine("Invalid input. Please enter a valid integer.");
            return;
        }

        var instance = await _taskHubClient.CreateOrchestrationInstanceAsync(
            typeof(SimpleOrchestration),
            input);

        _logger.LogInformation("Started SimpleOrchestration with ID: {InstanceId}", instance.InstanceId);
        await WaitForOrchestrationAsync(instance, stoppingToken);
    }


    /// <summary>
    /// Runs a complex orchestration that processes multiple integer values.
    /// Prompts the user to enter multiple numbers and waits for the orchestration to complete.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the operation.</param>
    private async Task RunComplexOrchestrationAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Enter integers (one per line). Enter empty line when done:");
        var numbers = new List<int>();

        while (true)
        {
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) break;

            if (int.TryParse(line, out int number))
            {
                numbers.Add(number);
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid integer.");
            }
        }

        if (numbers.Count == 0)
        {
            Console.WriteLine("No valid numbers entered.");
            return;
        }

        var instance = await _taskHubClient.CreateOrchestrationInstanceAsync(
            typeof(ComplexOrchestration),
            numbers);

        _logger.LogInformation("Started ComplexOrchestration with ID: {InstanceId}", instance.InstanceId);
        await WaitForOrchestrationAsync(instance, stoppingToken);
    }


    /// <summary>
    /// Waits for an orchestration to complete and logs its result.
    /// </summary>
    /// <param name="instanceId">The ID of the orchestration instance to wait for.</param>
    /// <param name="stoppingToken">Cancellation token to stop the operation.</param>
    private async Task WaitForOrchestrationAsync(OrchestrationInstance instance, CancellationToken stoppingToken)
    {
        Console.WriteLine("Waiting for orchestration to complete...");
        
        var state = await _taskHubClient.WaitForOrchestrationAsync(
            instance,
            TimeSpan.FromMinutes(5),
            stoppingToken);

        if (state.OrchestrationStatus == OrchestrationStatus.Completed)
        {
            Console.WriteLine($"Orchestration {instance.InstanceId} completed with result: {state.Output}");
            _logger.LogInformation("Orchestration {InstanceId} completed with result: {Result}",
                instance.InstanceId, state.Output);
        }
        else
        {
            Console.WriteLine($"Orchestration {instance.InstanceId} ended with status: {state.OrchestrationStatus}");
            _logger.LogWarning("Orchestration {InstanceId} ended with status: {Status}",
                instance.InstanceId, state.OrchestrationStatus);

        }
    }
} 