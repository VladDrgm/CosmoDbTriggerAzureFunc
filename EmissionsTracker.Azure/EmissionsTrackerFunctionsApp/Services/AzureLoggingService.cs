using Azure.Identity;
using Azure.ResourceManager;
using EmissionsTracker.Models;

namespace EmissionsTracker.Services;

public class AzureLoggingService()
{
    private readonly AppSettings _appSettings;

    public AzureLoggingService(AppSettings appSettings) : this()
    {
        _appSettings = appSettings;
    }

    private ClientSecretCredential ObtainCredentials()
    {
        return new ClientSecretCredential(_appSettings.TenantId, _appSettings.ClientId, _appSettings.ClientSecret);
    }

    public ArmClient GetArmClient()
    {
        var credential = ObtainCredentials();
        return new ArmClient(credential);
    }
}