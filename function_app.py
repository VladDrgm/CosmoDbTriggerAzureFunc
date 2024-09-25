import azure.functions as func
import logging

app = func.FunctionApp()

@app.cosmos_db_trigger(
    arg_name="azcosmosdb", 
    container_name="Items_Vlad",  # Should match the container name in Terraform
    database_name="test-db",  # Should match the database name in Terraform
    lease_collection_name="leases",  # Ensure this matches the created leases collection
    connection="cgcosmodbtrigcsdb_DOCUMENTDB"  # Connection string in your app settings
)
def cosmosdb_trigger(azcosmosdb: func.DocumentList):
    logging.info('Cosmos DB change feed triggered.')