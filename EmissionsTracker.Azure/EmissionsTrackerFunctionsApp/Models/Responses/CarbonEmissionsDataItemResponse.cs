using System.Text.Json.Serialization;

namespace EmissionsTracker.Models.Responses;

public class CarbonEmissionsDataItemResponse
{
    [JsonPropertyName("dataType")]
    public string DataType { get; set; }
    [JsonPropertyName("date")]
    public DateTimeOffset Date { get; set; }
    [JsonPropertyName("totalCarbonEmissions")]
    public double TotalCarbonEmissions { get; set; }
    [JsonPropertyName("totalCarbonEmission12MonthsAgo")]
    public double TotalCarbonEmission12MonthsAgo { get; set; }
    [JsonPropertyName("totalCarbonEmissionLastMonth")]
    public double TotalCarbonEmissionLastMonth { get; set; }
}
