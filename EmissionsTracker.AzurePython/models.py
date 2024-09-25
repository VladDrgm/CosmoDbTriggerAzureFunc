from dataclasses import dataclass, asdict
from typing import Optional, List

@dataclass
class AzureSubscription:
    subscription_id: Optional[str] = None
    subscription_display_name: Optional[str] = None

    @classmethod
    def from_api(cls, api_subscription) -> "AzureSubscription":
        """Factory method to create AzureSubscription from API response."""
        return cls(
            subscription_id=api_subscription.subscription_id,
            subscription_display_name=api_subscription.display_name
        )

@dataclass
class AzureTenant:
    id: Optional[str] = None
    display_name: Optional[str] = None
    subscriptions: Optional[List[AzureSubscription]] = None

    @classmethod
    def from_api(cls, api_tenant) -> "AzureTenant":
        """Factory method to create AzureTenant from API response."""
        return cls(
            id=api_tenant.tenant_id,
            display_name=api_tenant.display_name,
            subscriptions=[AzureSubscription.from_api(sub) for sub in api_tenant.subscriptions]
        )

@dataclass
class AppSettings:
    tenant_id: str
    client_id: str
    client_secret: str

# Azure Logging Service
class AzureLoggingService:
    def __init__(self, app_settings: AppSettings):
        self._app_settings = app_settings
        self._arm_client = None

    def obtain_credentials(self):
        return ClientSecretCredential(
            tenant_id=self._app_settings.tenant_id,
            client_id=self._app_settings.client_id,
            client_secret=self._app_settings.client_secret
        )

    def get_arm_client(self):
        if not self._arm_client:
            credential = self.obtain_credentials()
            self._arm_client = SubscriptionClient(credential)  # Initialize once, globally
        return self._arm_client

    def list_subscriptions(self):
        arm_client = self.get_arm_client()
        return list(arm_client.subscriptions.list())  # Get all subscriptions visible to the credentials

# Example usage:
app_settings = AppSettings(
    tenant_id='your-tenant-id',
    client_id='your-client-id',
    client_secret='your-client-secret'
)
