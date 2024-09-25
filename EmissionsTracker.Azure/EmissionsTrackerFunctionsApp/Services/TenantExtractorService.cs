using Azure.ResourceManager;

namespace EmissionsTracker.Services;

public static class TenantExtractorService
{
    public static async Task<List<TenantDto>> GetTenants(ArmClient client)
    {
        var tenants = new List<TenantDto>();

        var tenantResources = client.GetTenants().GetAllAsync();

        await foreach (var tenant in tenantResources)
        {
            if (tenant.Data.TenantId == Guid.Parse(Environment.GetEnvironmentVariable("TenantId")!)) // ONLY FOR DEMO PURPOSES
            {
                var subscriptions = tenant.GetSubscriptions().GetAllAsync();
                var dtoSubList = new List<SubscriptionDto>();

                await foreach (var sub in subscriptions)
                {
                    // Convert SubscriptionResource to SubscriptionDto
                    var dto = new SubscriptionDto(sub.Data.SubscriptionId, sub.Data.DisplayName);

                    dtoSubList.Add(dto);
                }

                var dtoTenant = new TenantDto
                {
                    TenantId = tenant.Data.TenantId,
                    TenantDisplayName = tenant.Data.DisplayName,
                    Subscriptions = dtoSubList
                };

                tenants.Add(dtoTenant);
            }
        }

        return tenants;
    }


    public record TenantDto
    {
        public required Guid? TenantId { get; set; }
        public required string TenantDisplayName { get; set; }
        public required List<SubscriptionDto> Subscriptions { get; set; }
    }

    public record SubscriptionDto(string Id, string DisplayName);
}