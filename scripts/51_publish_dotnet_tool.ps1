param(
    [Parameter(Mandatory = $false)]
    [string]$ApiKey = $env:NUGET_API_KEY
)

$ErrorActionPreference = "Stop"

# Ensure API key is available
if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    $ApiKey = Read-Host -Prompt "Enter NuGet API Key"
    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        Write-Error "NuGet API key is required to publish packages."
        exit 1
    }
}

$ScriptDir = $PSScriptRoot
$SourceUrl = "https://api.nuget.org/v3/index.json"

# 1. Build & Pack packages first
& "$ScriptDir/50_create_dotnet_tool.ps1"

# 2. Define project directories producing NuGet packages
$ProjectFolders = @(
    (Join-Path $ScriptDir "../src/Tool/BSMake"),
    (Join-Path $ScriptDir "../src/Tool/BurnSystems.Make.BuildAgent")
)

foreach ($Folder in $ProjectFolders) {
    $NupkgDir = Join-Path $Folder "nupkg"

    if (-not (Test-Path $NupkgDir)) {
        Write-Error "Package directory not found: $NupkgDir"
        continue
    }

    # Find the most recently created .nupkg file (excluding symbol packages)
    $LatestPackage = Get-ChildItem -Path $NupkgDir -Filter "*.nupkg" |
            Where-Object { $_.Name -notlike "*.snupkg" } |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1

    if ($null -eq $LatestPackage) {
        Write-Error "No .nupkg files found in $NupkgDir"
        continue
    }

    $confirmation = Read-Host -Prompt "Do you want to publish $($LatestPackage.Name) to NuGet? (y/N)"
    if ($confirmation -ne "y" -and $confirmation -ne "Y") {
        Write-Host "Skipping $($LatestPackage.Name)." -ForegroundColor Yellow
        continue
    }

    Write-Host "Publishing package: $($LatestPackage.FullName)..." -ForegroundColor Cyan

    dotnet nuget push $LatestPackage.FullName `
        --source $SourceUrl `
        --api-key $ApiKey `
        --skip-duplicate

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to push $($LatestPackage.Name)"
        exit $LASTEXITCODE
    }
}

Write-Host "All packages published successfully." -ForegroundColor Green