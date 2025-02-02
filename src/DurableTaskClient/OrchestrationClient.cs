using DurableTask.Core;
using DurableTaskSamples.Orchestrations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DurableTaskClient;

public sealed class OrchestrationClient(
    ILogger<OrchestrationClient> logger,
    TaskHubClient taskHubClient)
    : BackgroundService
{
    private readonly ILogger<OrchestrationClient> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly TaskHubClient _taskHubClient = taskHubClient ?? throw new ArgumentNullException(nameof(taskHubClient));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Schedule a SimpleOrchestration
                var input = 42;  // Simple integer input
                var instanceId = await _taskHubClient.CreateOrchestrationInstanceAsync(
                    typeof(SimpleOrchestration),
                    input);

                _logger.LogInformation("Started SimpleOrchestration with ID: {InstanceId}", instanceId);

                // Wait for orchestration to complete
                var state = await _taskHubClient.WaitForOrchestrationAsync(
                    instanceId,
                    TimeSpan.FromMinutes(1),
                    stoppingToken);

                if (state.OrchestrationStatus == OrchestrationStatus.Completed)
                {
                    _logger.LogInformation("Orchestration {InstanceId} completed with result: {Result}",
                        instanceId, state.Output);
                }
                else
                {
                    _logger.LogWarning("Orchestration {InstanceId} ended with status: {Status}",
                        instanceId, state.OrchestrationStatus);
                }

                // Wait before starting next orchestration
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing orchestration");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
} 