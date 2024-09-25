# Azure Provider

provider "azurerm" {
  features {}
  # use env variable
  subscription_id = var.subscription_id
}
