param(
	[Parameter(Mandatory=$true)]
	[string] $ResourceGroupName,
	[string] $DefaultResourceName = "snowflakestoragedemo",
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

        $parameters["keyVaultName"] = $DefaultResourceName
        $parameters["storageName"] = $DefaultResourceName  
		
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

	Write-Host "`nStep3: Uploading files to storage account"  -ForegroundColor Green
    try {
		
		$storageContainerName = "data"

		$storageAccount = (Get-AzureRmStorageAccount | Where-Object{$_.StorageAccountName -eq $DefaultResourceName})
		New-AzureStorageContainer -Name $storageContainerName -Context $storageAccount.Context -ErrorAction SilentlyContinue *>&1

		$dataFolder = "$ArtifactStagingDirectory/Data"

		$dataFilePaths = Get-ChildItem "$dataFolder" -Recurse -File | ForEach-Object -Process {$_.FullName}
		foreach ($SourcePath in $dataFilePaths) {
			Set-AzureStorageBlobContent -File $SourcePath -Blob $SourcePath.Substring($dataFolder.Length + 1) `
				-Container $storageContainerName -Context $storageAccount.Context -Force
		}
    }	
	catch{
		throw $_
	}

	Write-Host "`nStep4: Removing artifact storage account"  -ForegroundColor Green
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