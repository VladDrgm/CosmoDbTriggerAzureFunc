variable "resource_group_name" {
    default = "VladTest"
}

variable "location" {
    default = "West Europe"
}

variable "cosmos_db_name" {
  default = "cg-cosmodbtrig-cdb"
}

variable "function_app_name" {
  default = "cg-cosmodbtrig-fa"
}

variable "app_service_plan_name" {
  default = "cg-cosmodbtrig-asp"
}
