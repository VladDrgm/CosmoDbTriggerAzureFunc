using System.Text.Json;
using EmissionsTracker.Models;
using EmissionsTracker.Models.Requests;
using EmissionsTracker.Models.Responses;
using EmissionsTracker.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace EmissionsTracker;

public class DurableFunctionOrchestratorMain
{
    private readonly HttpClient _client;
    private readonly AzureLoggingService _loggerService;
    private readonly AppSettings _appSettings;
    private readonly ILogger _logger;

    public DurableFunctionOrchestratorMain(ILoggerFactory loggerFactory, AppSettings appSettings, AzureLoggingService loggerService, HttpClient client)
    {
        _logger = loggerFactory.CreateLogger<DurableFunctionOrchestratorMain>();
        _appSettings = appSettings;
        _loggerService = loggerService;
        _client = client;
    }

    [Function(nameof(DurableFunctionOrchestratorMain))]
    public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(DurableFunctionOrchestratorMain));
        logger.LogInformation("Starting Orchestration");

        // Replace name and input with values relevant for your Durable Functions Activity
        // Step 1: Get the list of tenants
        var tenants = await context.CallActivityAsync<List<TenantExtractorService.TenantDto>>(nameof(GetTenantsActivity), Guid.NewGuid());

        var tasks = new List<Task<CarbonEmissions>>();

        // Step 2: For each tenant, get the subscriptions and retrieve carbon emissions data
        foreach (var tenant in tenants)
        {
            logger.LogInformation("Obtaining subscriptions for tenant: " + tenant.TenantId);
            // Call activity to get subscriptions for a tenant
            var request = new CarbonEmissionsRequest
            {
                TenantId = tenant.TenantId,
                TenantName = tenant.TenantDisplayName ?? "N/A",
                Subscriptions = tenant.Subscriptions.Select(s => s.Id).ToArray(),
                FromDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-6),
                ToDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1)
            };

            // Process each subscription in parallel
            var task = context.CallActivityAsync<CarbonEmissions>(nameof(GetCarbonEmissionsActivityFunc.GetCarbonEmissionsActivity), request);
            tasks.Add(task);
        }

        // Step 3: Wait for all tasks to complete and collect the results
        await Task.WhenAll(tasks);

        logger.LogInformation("Processing finished. Data collected and stored in Data.txt");

        // Step 4: Flatten the results and log or store them as needed
        var finalData = tasks
            .Select(task => task.Result)
            .GroupBy(response => response.TenantId)
            .ToList();

        // Serialize the data to JSON
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(finalData, options);

        // Log the JSON-formatted data
        // logger.LogInformation($"Emissions data: {json}");

        // Save data to file
        File.WriteAllText(@"Data.txt", json);
    }

    [Function("DurableFunctionsOrchestrationCSharp1_TimerStart")]
    public async Task TimerStart(
        [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, [DurableClient] DurableTaskClient client, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("DurableFunctionsOrchestrationCSharp1_TimerStart");

        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(DurableFunctionOrchestratorMain));
        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

    }

    [Function(nameof(GetTenantsActivity))]
    public async Task<List<TenantExtractorService.TenantDto>> GetTenantsActivity([ActivityTrigger] Guid? input, FunctionContext executionContext)
    {
        // Use Azure SDK to get all tenants accessible by the service principal
        var armClient = _loggerService.GetArmClient();

        var tenants = await TenantExtractorService.GetTenants(armClient);

        return tenants;
    }
}
