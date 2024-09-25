using EmissionsTracker.Models;
using EmissionsTracker.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();
        services.AddHttpClient();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables() // Adds environment variables as a source
            .Build();

        // bind to AppSettings
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));

        // Add AppSettings as a transient service
        services.AddTransient(provider => provider.GetService<IOptions<AppSettings>>().Value);

        // Create AzureLoggingService singleton with AppSettings
        services.AddSingleton(provider => new AzureLoggingService(provider.GetService<IOptions<AppSettings>>().Value));
    })
    .Build();

host.Run();
