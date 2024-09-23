# Cosmos DB Account
resource "azurerm_cosmosdb_account" "cosmosdb" {
  name                = var.cosmos_db_name
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  is_virtual_network_filter_enabled = false  # Disable network restrictions

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = azurerm_resource_group.rg.location
    failover_priority = 0
  }

  geo_location {
    location          = "West Us"  # Replace with the secondary location
    failover_priority = 1  # Setting priority for failover
  }

  ip_range_filter = []  # Empty list means no IP filtering, allowing public access
}
