namespace EmissionsTracker.Models.Requests;

public record CarbonEmissionsRequest
{
    public required Guid? TenantId { get; init; }
    public required string TenantName { get; init; }
    public required string[] Subscriptions { get; init; }
    public required DateTime FromDate { get; init; }
    public required DateTime ToDate { get; init; }
}