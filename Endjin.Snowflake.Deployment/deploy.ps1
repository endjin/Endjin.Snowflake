param(
	[Parameter(Mandatory=$true)]
	[string] $ResourceGroupName,
	[string] $DefaultResourceName = "snowflakeconnector",
    [string] $ResourceGroupLocation = "uksouth",
    [string] $ArtifactStorageContainerName = "stageartifacts",
    [string] $ArtifactStagingDirectory = $PSScriptRoot,
	[Parameter(Mandatory=$true)]
	[securestring] $SnowflakeConnectionString
)

Begin {
    # Setup options and variables
    $ErrorActionPreference = 'Stop'
    Set-Location $PSScriptRoot
    $ArtifactStorageResourceGroupName = $ResourceGroupName;
    $ArtifactStorageAccountName = $DefaultResourceName + "ar"
}

Process {
    # Create resource group and artifact storage account

    Write-Host "`nStep1: Creating resource group $ResourceGroupName and artifact storage account $ArtifactStorageAccountName" -ForegroundColor Green
    try {
        .\Scripts\Create-StorageAccount.ps1 `
            -ResourceGroupName $ArtifactStorageResourceGroupName `
            -ResourceGroupLocation $ResourceGroupLocation `
            -StorageAccountName $ArtifactStorageAccountName
    }
    catch {
        throw $_
    }

    # Deploy main ARM template

    Write-Host "`nStep2: Deploying main resources template"  -ForegroundColor Green
    try {
        $parameters = New-Object -TypeName Hashtable

        $parameters["appInsightsName"] = $DefaultResourceName
        $parameters["keyVaultName"] = $DefaultResourceName
        $parameters["storageName"] = $DefaultResourceName
        $parameters["snowflakeConnectionString"] = $SnowflakeConnectionString
        $parameters["functionsAppName"] = $DefaultResourceName + "func"

        $TemplateFilePath = [System.IO.Path]::Combine($ArtifactStagingDirectory, "deploy.json")

        $str = $parameters | Out-String
        Write-Host $str

        Write-Host $ArtifactStagingDirectory

        $deploymentResult = .\Scripts\Deploy-AzureResourceGroup.ps1 `
            -UploadArtifacts `
            -ResourceGroupLocation $ResourceGroupLocation `
            -ResourceGroupName $ResourceGroupName `
            -StorageAccountName $ArtifactStorageAccountName `
            -ArtifactStagingDirectory $ArtifactStagingDirectory `
            -StorageContainerName $ArtifactStorageContainerName `
            -TemplateParameters $parameters `
            -TemplateFile $TemplateFilePath

		$appInsightsName = $deploymentResult.outputs.appInsightsName.value
		$functionsAppName = $deploymentResult.outputs.functionsAppName.value
    }
    catch {
        throw $_
    }

	Write-Host "`nStep3: Removing artifact storage account"  -ForegroundColor Green
    try {
        Remove-AzureRmStorageAccount -ResourceGroupName $ResourceGroupName -AccountName $ArtifactStorageAccountName -Force
	}
	catch{
		throw $_
	}
	
	$appInsightsApplicationId = $deploymentResult.outputs.appInsightsApplicationId.value

	# Set ADO variables
	Write-Host "##vso[task.setvariable variable=AppInsightsApplicationId;]$appInsightsApplicationId"
	Write-Host "##vso[task.setvariable variable=AppInsightsReleaseAnnotationsApiKey;issecret=true;]$appInsightsReleaseAnnotationsApiKey"
	Write-Host "##vso[task.setvariable variable=FunctionsAppName;]$functionsAppName"
}

End {
    Write-Host -ForegroundColor Green "`n######################################################################`n"
    Write-Host -ForegroundColor Green "Deployment finished"
    Write-Host -ForegroundColor Green "`n######################################################################`n"
}