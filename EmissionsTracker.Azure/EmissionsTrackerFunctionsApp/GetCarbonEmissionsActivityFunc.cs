using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EmissionsTracker.Models;
using EmissionsTracker.Models.Requests;
using EmissionsTracker.Models.Responses;
using Microsoft.Azure.Functions.Worker;

namespace EmissionsTracker;

public class GetCarbonEmissionsActivityFunc
{
    private readonly AppSettings _appSettings;
    private readonly HttpClient _client;
    private const string Url = "https://management.azure.com/providers/Microsoft.Carbon/carbonEmissionReports?api-version=2023-04-01-preview";

    public GetCarbonEmissionsActivityFunc(AppSettings appSettings, HttpClient client)
    {
        _appSettings = appSettings;
        _client = client;
    }

    [Function(nameof(GetCarbonEmissionsActivity))]
    public async Task<CarbonEmissions> GetCarbonEmissionsActivity([ActivityTrigger] CarbonEmissionsRequest request, FunctionContext executionContextt)
    {
        // prepare content for Body
        var requestBody = new
        {
            // reportType = "OverallSummaryReport",  // -> we can alternate the report type, but we would need to adjust dates
            reportType = "MonthlySummaryReport",
            subscriptionList = request.Subscriptions,
            carbonScopeList = new[] { "Scope1", "Scope2", "Scope3" },
            dateRange = new
            {
                start = request.FromDate.ToString("yyyy-MM-dd"),
                end = request.ToDate.ToString("yyyy-MM-dd")
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var clientId = _appSettings.ClientId;
        var clientSecret = _appSettings.ClientSecret;
        var tenantId = request.TenantId;

        var token = await GetAzureTokenAsync(tenantId, clientId, clientSecret);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // send request with client credentials + body prepared; Azure Carbon Emissions API accepts both GET and POST
        var response = await _client.PostAsync(Url, content);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<CarbonEmissionsResponse>(data)!;

        return new(tenantId, request.TenantName, request.Subscriptions, result.Values);
    }

    private async Task<string> GetAzureTokenAsync(Guid? tenantId, string clientId, string clientSecret)
    {
        var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";
        var data = new[]
        {
             new KeyValuePair<string, string>("grant_type", "client_credentials"),
             new KeyValuePair<string, string>("client_id", clientId),
             new KeyValuePair<string, string>("client_secret", clientSecret),
             new KeyValuePair<string, string>("resource", "https://management.azure.com")
         };
        var client = _client;
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        var formData = new FormUrlEncodedContent(data);
        request.Content = formData;
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        var tokenResponse = JsonSerializer.Deserialize<AzureTokenResponse>(responseBody)!;

        return tokenResponse.AccessToken;
    }
}