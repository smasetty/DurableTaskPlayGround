namespace DurableTaskWorker;

/// <summary>
/// The main class for the DTFxWorker application.
/// </summary>
internal class Program
{
    private static readonly ManualResetEvent quitEvent =
            new ManualResetEvent(false);

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">An array of command-line arguments.</param>
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var worker = new DurableTaskWorker();

        try
        {
            Console.WriteLine("Initializing worker");
            await worker.StartAsync();

            Console.WriteLine("Started TaskHubWorker, press Ctrl-C to stop");
            quitEvent.WaitOne();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            worker.Stop();
        }
    }
}
