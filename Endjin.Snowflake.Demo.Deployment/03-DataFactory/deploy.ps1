param(
	[Parameter(Mandatory=$true)]
	[string] $ResourceGroupName,
	[string] $DefaultResourceName = "snowflakedatafactory",
	[Parameter(Mandatory=$true)]
	[string] $SnowflakeConnectorFunctionUrl,
	[Parameter(Mandatory=$true)]
	[securestring] $SnowflakeConnectorFunctionKey,
    [string] $ResourceGroupLocation = "uksouth",
    [string] $ArtifactStorageContainerName = "stageartifacts",
    [string] $ArtifactStagingDirectory = $PSScriptRoot
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
        ..\Scripts\Create-StorageAccount.ps1 `
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
        $parameters["dataFactoryName"] = $DefaultResourceName
        $parameters["snowflakeConnectorFunctionKey"] = $SnowflakeConnectorFunctionKey  
        $parameters["snowflakeConnectorFunctionUrl"] = $SnowflakeConnectorFunctionUrl  
		
		$TemplateFilePath = [System.IO.Path]::Combine($PSScriptRoot, "deploy.json")		

        $str = $parameters | Out-String
        Write-Host $str

        Write-Host $ArtifactStagingDirectory

        $deploymentResult = ..\Scripts\Deploy-AzureResourceGroup.ps1 `
            -UploadArtifacts `
            -ResourceGroupLocation $ResourceGroupLocation `
            -ResourceGroupName $ResourceGroupName `
            -StorageAccountName $ArtifactStorageAccountName `
            -ArtifactStagingDirectory $ArtifactStagingDirectory `
            -StorageContainerName $ArtifactStorageContainerName `
            -TemplateParameters $parameters `
            -TemplateFile $TemplateFilePath
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
}

End {
    Write-Host -ForegroundColor Green "`n######################################################################`n"
    Write-Host -ForegroundColor Green "Deployment finished"
    Write-Host -ForegroundColor Green "`n######################################################################`n"
}