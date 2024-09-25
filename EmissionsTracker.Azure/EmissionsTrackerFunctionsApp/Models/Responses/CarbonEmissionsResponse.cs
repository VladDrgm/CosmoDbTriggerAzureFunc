using System.Text.Json.Serialization;

namespace EmissionsTracker.Models.Responses;

public record CarbonEmissionsResponse
{
    [JsonPropertyName("value")]
    public List<CarbonEmissionsDataItemResponse> Values { get; init; } = [];
}

public record CarbonEmissions(Guid? TenantId, string TenantName, string[] Subscriptions, List<CarbonEmissionsDataItemResponse> Values);