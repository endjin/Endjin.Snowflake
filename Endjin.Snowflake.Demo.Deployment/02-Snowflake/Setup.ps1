<#
This script sets up a new snowflake database.

The script performs the following:

1. Creates a database
2. Create a schema called SALES
3. Creates a table called LINEITEM
4. Creates a view called SupplierAgg that does some aggregation over the LINEITEM table
5. Creates a stage called azure_adf_stage pointing at an Azure Storage container
#>

[CmdletBinding(DefaultParametersetName='None')] 
param(	
	[Parameter(Mandatory=$true)]
	[string] $ConnectionString,
	[Parameter(Mandatory=$true)]
	[string] $Warehouse,
	[Parameter(Mandatory=$true)]
	[string] $DatabaseName,
	[Parameter(Mandatory=$true)]
	[string] $AzureStorageContainerUrl,
	[Parameter(Mandatory=$true)]
	[string] $SASToken
)

Set-Location $PSScriptRoot

$AzureStorageContainerUrl = $AzureStorageContainerUrl.Replace("https://", "azure://")

try {
	if (![System.IO.File]::Exists((Join-Path $PSScriptRoot "..\..\Endjin.Snowflake\bin\Debug\netstandard2.0\publish\Endjin.Snowflake.dll"))){
		dotnet publish ..\Endjin.Snowflake --self-contained
	}
	
	Add-Type -Path '..\..\Endjin.Snowflake\bin\Debug\netstandard2.0\publish\Endjin.Snowflake.dll'
	Add-Type -Path '..\..\Endjin.Snowflake\bin\Debug\netstandard2.0\publish\Snowflake.Data.dll'
	
	[System.Reflection.Assembly]::LoadWithPartialName("Endjin.Snowflake")
	[System.Reflection.Assembly]::LoadWithPartialName("Snowflake.Data")

	$client = New-Object -TypeName Endjin.Snowflake.SnowflakeClient -ArgumentList @($ConnectionString)
	$setupScript = [System.IO.File]::ReadAllText((Join-Path $PSScriptRoot "Snowflake.sql"))
	$statements = $setupScript.Replace("#DATABASE#", $DatabaseName).Replace('#WAREHOUSE#', $Warehouse).Replace("#AZURESTORAGECONTAINERURL#", $AzureStorageContainerUrl).Replace("#SASTOKEN#", $SASToken).Split(";") | % { $_.Trim() } | where { $_ -ne ""}
	$client.ExecuteNonQuery(([string[]]$statements)) | Out-Null	
	Write-Host "Snowflake playground environment successfully created" -ForegroundColor DarkGreen
}
catch{
	throw $_
}

