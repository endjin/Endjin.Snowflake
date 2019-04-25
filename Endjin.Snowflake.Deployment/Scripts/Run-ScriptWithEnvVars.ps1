param(
	[string] $ScriptPath,
	[string] $EnvVarPrefix,
	[hashtable] $SecretVariables,
	[string] $TemplatePath
)

Set-Location $PSScriptRoot

$rawParams = @{}
$scriptParams = @{}
$templateParams = @{}

# Add all environment variables to the raw parameters hashtable
if ($EnvVarPrefix)
{
	# If prefix is defined, only include environment variables that start with the prefix
	(Get-ChildItem env:) | Where-Object {$_.Name.StartsWith($EnvVarPrefix, "InvariantCultureIgnoreCase") } | Foreach-Object { $rawParams[$_.Name.Substring($EnvVarPrefix.Length)] = $_.Value }
} 
else {
	(Get-ChildItem env:) | Foreach-Object { $rawParams[$_.Name] = $_.Value }
}

# Add all secret variables to the raw parameters hashtable
if ($SecretVariables)
{
	$SecretVariables.GetEnumerator() | Foreach-Object { $rawParams[$_.Name] = (convertto-securestring $_.Value -asplaintext -force) }
}

# Get all script parameters
$scriptParameters = (Get-Command $ScriptPath).Parameters

# Strip the parameters hashtable down to the required set of parameters for the script
$rawParams.GetEnumerator() | Foreach-Object { if ($scriptParameters.$($_.Name)) { $scriptParams.Add($_.Name, $_.Value) } }

# If script has 'TemplateParameters' parameter (of type HashTable), and template path is provided, then build up 'TemplateParameters' hash table
$templateParametersParameter = $scriptParameters.'TemplateParameters';

$buildTemplateParameters = $templateParametersParameter -and $templateParametersParameter.ParameterType -eq [HashTable] -and $TemplatePath

if ($buildTemplateParameters)
{
	$templateParameters = (Get-Content $TemplatePath | ConvertFrom-Json).parameters
	$rawParams.GetEnumerator() | 
		Foreach-Object { 
			if ($templateParameters.$($_.Name)) { 

				$value = $_.Value

				if ($templateParameters.$($_.Name).type -eq "bool") {
					$value = [System.Convert]::ToBoolean($value)
				} elseif ($templateParameters.$($_.Name).type -eq "int") {
					$value = [System.Convert]::ToInt32($value)
				}

				$templateParams.Add($_.Name, $value)
			} 
		}

	$scriptParams.'TemplateParameters' = $templateParams
}

Write-Host "Script parameters:"
Write-Host ($scriptParams | Out-String)

if ($buildTemplateParameters)
{
	Write-Host
	Write-Host "Template parameters:"
	Write-Host ($templateParams | Out-String)
}

& $ScriptPath @scriptParams