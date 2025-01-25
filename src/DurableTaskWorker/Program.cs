using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace DurableTaskWorker;

/// <summary>
/// The main class for the DTFxWorker application.
/// </summary>
internal class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        using IHost host = CreateHostBuilder(args).Build();
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        await host.RunAsync(cts.Token);
    }

    /// <summary>
    /// Creates and configures an IHostBuilder instance.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>An IHostBuilder instance.</returns>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                var envVariable = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (!string.IsNullOrEmpty(envVariable))
                {
                    hostingContext.HostingEnvironment.EnvironmentName = envVariable;
                }

                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                      .AddEnvironmentVariables()
                      .AddCommandLine(args);
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureServices((_, services) =>
                services.AddHostedService<Worker>()
                    .AddCustomLogging()
                    .AddDurableTaskWorker());
}

/// <summary>
/// Worker service that runs the DurableTaskWorker.
/// </summary>
internal class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DurableTaskWorker _worker;

    /// <summary>
    /// Initializes a new instance of the <see cref="Worker"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    public Worker(ILogger<Worker> logger, DurableTaskWorker worker)
    {
        _logger = logger;
        _worker = worker;
    }

    /// <summary>
    /// Executes the worker service asynchronously.
    /// </summary>
    /// <param name="stoppingToken">A token that can be used to signal cancellation of the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Initializing Worker");

        try
        {
            await _worker.StartAsync();

            _logger.LogInformation("Started TaskHubWorker, press ctrl-c to stop");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Task was canceled.");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while running the worker.");
        }
        finally
        {
            await _worker.StopAsync();
        }
    }
}
