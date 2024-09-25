import azure.functions as func
import azure.durable_functions as df
from azure.identity import ClientSecretCredential
from azure.mgmt.resource.subscriptions import SubscriptionClient
import requests
from dotenv import load_dotenv
import os


import models

myApp = df.DFApp(http_auth_level=func.AuthLevel.ANONYMOUS)

# use dotenv to load environment variables
dotenv_path = '.env'
load_dotenv(dotenv_path)

# An HTTP-triggered function with a Durable Functions client binding
@myApp.route(route="orchestrators/{functionName}")
@myApp.durable_client_input(client_name="client")
async def http_start(req: func.HttpRequest, client):
    function_name = req.route_params.get('functionName')
    instance_id = await client.start_new(function_name)
    response = client.create_check_status_response(req, instance_id)
    return response

# Orchestrator
@myApp.orchestration_trigger(context_name="context")
def azure_emissions_orchestrator(context):
    tenants = yield context.call_activity("get_tenants_and_subscriptions")
    emissions = []
    for tenant in tenants:
        print(tenant)


@myApp.activity_trigger(input_name="tenant")
def get_emissions_data(tenant: dict):
    tenant_id = tenant.get("tenant_id") 
    
    if tenant_id == "Tenant1":
        return 1
    elif tenant_id == "Tenant2":
        return 2
    # return get_emission(tenant)

@myApp.activity_trigger(input_name="placeholder")
def get_tenants_and_subscriptions(placeholder: str):
    tenant_id = os.getenv("AZURE_TENANT_ID")
    client_id = os.getenv("AZURE_CLIENT_ID")
    client_secret = os.getenv("AZURE_CLIENT_SECRET")

    # Create a credential object
    credential = ClientSecretCredential(
        tenant_id=tenant_id,
        client_id=client_id,
        client_secret=client_secret
    )

    # Create a subscription client object
    subscription_client = SubscriptionClient(credential)

    # Fetch all subscriptions for the tenant
    subscriptions = list(subscription_client.subscriptions.list())
    
    # Create a list to hold tenant information
    tenants = []

    # Loop through the subscriptions to extract tenant information
    # This needs re-doing. We get 1 tenant and 1 subscription, but a tenant can have multiple subscriptions, so we need to loop through them and group them by tenant
    for sub in subscriptions:
        tenant_info = {
            "tenant_id": sub.tenant_id,  # Extracting tenant ID from subscription
            "tenant_display_name": sub.display_name,
            "subscriptions": [
                {
                    "subscription_id": sub.subscription_id,
                    "subscription_display_name": sub.display_name
                }
            ]
        }
        # Check if this tenant is already in the list, if not, add it
        if not any(t['tenant_id'] == tenant_info['tenant_id'] for t in tenants):
            tenants.append(tenant_info)
    
    print("Retrieved " + str(len(tenants)) + " tenants with their subscriptions.")
    
    return tenants  # Return tenant data for the orchestrator to use
