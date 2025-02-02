using Common.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DurableTaskClient;

/// <summary>
/// Entry point for the Durable Task Client application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    /// <summary>
    /// Creates and configures the host builder for the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A configured IHostBuilder instance.</returns>
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
                // TODO: cleanup the logging mess
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<ILoggerService, Logger>();
                services.AddDurableTaskClient(context.Configuration);
                services.AddHostedService<OrchestrationClient>();
            });
} 