using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using T4.TraceListener.Logging;
using T4ExampleLinuxCs;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = AppStartup();
        var app = ActivatorUtilities.CreateInstance<App>(host.Services);
        await app.RunAsync();

    }
    static void BuildConfig(IConfigurationBuilder builder)
    {
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();


    }

    static IHost AppStartup()
    {
        var builder = new HostApplicationBuilder();
        BuildConfig(builder.Configuration);
        // Configure logging.
        /*
         *You can configure the trace path in the file appsettings.json using the property T4Logging.LogFilePath
        Example:
        {
          "T4Logging": {
            "LogFilePath": "FolderName/SubFolderName"
          }
        }
         * */
        builder.Logging
            .ClearProviders()
            .AddConsole()
            .AddProvider(new T4LoggerProvider());
        var host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddLogging();
                    })
                    .Build();

        return host;
    }
}
