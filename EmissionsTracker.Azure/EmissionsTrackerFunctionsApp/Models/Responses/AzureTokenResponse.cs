using System.Text.Json.Serialization;

namespace EmissionsTracker.Models.Responses;

public class AzureTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}