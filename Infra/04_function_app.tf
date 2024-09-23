# Function App

resource "azurerm_function_app" "function_app" {
  name                       = var.function_app_name
  location                   = azurerm_resource_group.rg.location
  resource_group_name         = azurerm_resource_group.rg.name
  app_service_plan_id         = azurerm_app_service_plan.asp.id
  storage_account_name        = azurerm_storage_account.sa.name
  storage_account_access_key  = azurerm_storage_account.sa.primary_access_key
  os_type                     = "Linux"

  site_config {
    linux_fx_version = "python|3.8" # Example runtime
  }

  identity {
    type = "SystemAssigned"
  }
}
