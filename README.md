# Azure Data Factory Snowflake Connector

The Azure Data Factory Snowflake Connector allows you orchestrate data movement to and from a Snowflake account to an external cloud stage such as Azure, AWS or GCP.

The connector was created due to the current lack of native connector in Azure Data Factory. While this was the primary motivation, the connector can be called from almost any service that understands HTTP.

The Snowflake Connector comprises of two Azure Functions:

- `v1/load` Loads a file or set of files from a cloud stage (Azure Storage Account, S3 Bucket)
- `v1/unload` Executes and unloads the results of a Snowflake query into an external cloud stage

---

## Installing the connector

1. Deploy the Snowflake Connector Function App

From the `Endjin.Snowflake.Deployment` folder run:

 `.\deploy.ps1 -ResourceGroupName <resource group name> -DefaultResourceName <the default name of the resources>`

Note that the name of the function app that is created will be `<DefaultResourceName>func`.

2. Build and publish the functions

Ensure you have the latest `func` CLI tools installed:

`npm install -g azure-functions-core-tools@core`

From the `Endjin.Snowflake.Host` folder run:

`func azure functionapp publish <function-app>`

---

## How it works

The connector exposes two HTTP endpoints:

- `v1/load` 
- `v1/unload` 

Both require a `x-functions-key` header. 

The documentation for these endpoints can be found in the [OpenApi yaml definition](Endjin.Snowflake.Host/yaml/v1/snowflake.yaml) in this project.



Loading a file into snowflake:

```
POST
{
    "database": "ADF_DB",
    "schema": "SALES",
    "stage": "azure_adf_stage",
    "targetTable": "LINEITEM",
    "files": [
        "input/input.csv"
    ]
}
```

Loading a file into snowflake:

```
POST v1/load
{
    "database": "ADF_DB",
    "schema": "SALES",
    "stage": "azure_adf_stage",
    "targetTable": "LINEITEM",
    "files": [
        "input/input.csv"
    ],
    force: true
}
```

Unloading a file into snowflake:
```
POST v1/load

{
    "database":"ADF_DB",
    "schema": "SALES",
    "stage": "azure_adf_stage",
    "query": "select * from SupplierAgg",
    "filePrefix": "/output/Supplier.csv.gzip",
    "singleFile": true,
    "overwrite": true
}
```

---

## Known Limitations

The following Snowflake features are not currently supported:

- Overriding file format per operation - file Format must currently be specified at the stage level
- Specifying file patterns during load
- Specifying max file size during unload
- Validation mode

---

## Sample Data Factory

The solution includes a sample commandline application and Azure Data Factory.

The sample comprises of:

- An Azure Storage Account that will act as the Snowflake external stage
- A Snowflake database and associated objects
- An Azure Data Factory and pipeline that calls the connector to load and unload data from Snowflake
- A simple CLI that uses the Snowflake Client to load and unload data from Snowflake

To deploy the sample:

Create the Storage Account

From the `Endjin.Snowflake.Demo.Deployment\01-Storage` folder run:

`.\deploy.ps1 -ResourceGroupName <resource group name> -DefaultResourceName <the name of the new storage account / key vault>`
 
Setup the Snowflake environment

From the `Endjin.Snowflake.Demo.Deployment\02-Snowflake` folder run:

`.\Setup.ps1 -ConnectionString <your snowflake account connection string> -Warehouse <name of the snowflake warehouse to use> -DatabaseName <name of the database that the demo will use> -AzureStorageContainerUrl <url to a the storage account container setup in the previous step> -SASToken <a sas token for the account created in the previous step>`

The above will create a new database if one does not already exist.
It will also create:

- A schema called `SALES`
- A table called `LINEITEMS`
- A view called `SupplierAgg`
- A stage called `azure-adf-stage` (this will be associated with the Azure storage account created earlier)

Deploy the sample data factory

From the `Endjin.Snowflake.Demo.Deployment\03-DataFactory` folder run:

`.\deploy.ps1 -ResourceGroupName <resource group name> -DefaultResourceName <the name of the datafactory> -SnowflakeConnectorFunctionUrl <the url of the Snowflake Connector function app> -SnowflakeConnectorFunctionKey <the host key of the Snowflake Connector function app>`


The Data Factory Pipeline can be run from the Azure Portal or via Powershell / SDK. 

The following parameters are required when running the sample pipeline:

- `inputPath` a path to the file to load relative to the Azure Storage Account container
- `outputPath` the path to the output file on unload
- `database` the name of the database that was specified when deploying the sample
- `warehouse` the name of the Snowflake warehouse to use




