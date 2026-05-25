[CmdletBinding()]
param(
    [string]$PackagePath,
    [string]$PackageName = "VoiceInk.Windows",
    [switch]$Execute,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\smoke-msix-install.ps1 -PackagePath path [-PackageName VoiceInk.Windows] [-Execute]"
    Write-Host ""
    Write-Host "Prints a signed MSIX install/query/uninstall smoke plan by default."
    Write-Host "Pass -Execute only on a test machine where the signing certificate is already trusted."
    Write-Host "PackagePath must be a .msix under VoiceInk.Windows\artifacts."
    Write-Host "This script does not create or import certificates, sign packages, build packages, or trust certificates."
}

function Assert-PathInside {
    param(
        [string]$CandidatePath,
        [string]$RootPath,
        [string]$Message
    )

    $resolvedRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $resolvedCandidate = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    if (!$resolvedCandidate.StartsWith($resolvedRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase) -and
        ![string]::Equals($resolvedCandidate, $resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Message Path: $resolvedCandidate Root: $resolvedRoot"
    }
}

function Assert-NoReparsePointInPath {
    param(
        [string]$CandidatePath,
        [string]$RootPath
    )

    $resolvedRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $resolvedCandidate = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $currentPath = $resolvedCandidate
    while ($currentPath.Length -ge $resolvedRoot.Length) {
        if (Test-Path -LiteralPath $currentPath) {
            $item = Get-Item -LiteralPath $currentPath -Force
            if (($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -ne 0) {
                throw "Refusing to use reparse-point path for MSIX install smoke. Path: $currentPath"
            }
        }

        if ([string]::Equals($currentPath, $resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
            break
        }

        $parentPath = [System.IO.Directory]::GetParent($currentPath)
        if ($null -eq $parentPath) {
            break
        }

        $currentPath = $parentPath.FullName.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    }
}

function Write-SmokePlan {
    param(
        [string]$ResolvedPackagePath,
        [string]$ResolvedPackageName
    )

    Write-Host "MSIX install smoke plan:"
    Write-Host "  Package: $ResolvedPackagePath"
    Write-Host "  Package name: $ResolvedPackageName"
    Write-Host ""
    Write-Host "Prerequisite: the package must already be signed and the signing certificate must already be trusted on this test machine."
    Write-Host "This script does not create or import certificates."
    Write-Host ""
    Write-Host "Commands:"
    Write-Host "  Add-AppxPackage -Path `"$ResolvedPackagePath`""
    Write-Host "  `$packages = @(Get-AppxPackage -Name `"$ResolvedPackageName`")"
    Write-Host "  Get-AppxPackage -Name `"$ResolvedPackageName`""
    Write-Host "  Remove-AppxPackage -Package `$packages[0].PackageFullName"
    Write-Host ""
    Write-Host "Pass -Execute to run this smoke on a disposable test install."
}

if ($Help) {
    Show-Usage
    exit 0
}

if ([string]::IsNullOrWhiteSpace($PackagePath)) {
    throw "PackagePath is required unless -Help is used."
}

if ([string]::IsNullOrWhiteSpace($PackageName)) {
    throw "PackageName cannot be blank."
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"

if (![System.IO.Path]::IsPathRooted($PackagePath)) {
    $PackagePath = Join-Path $repoRoot $PackagePath
}

$resolvedPackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
Assert-PathInside `
    -CandidatePath $resolvedPackagePath `
    -RootPath $artifactsRoot `
    -Message "Refusing to smoke package outside artifact root."
Assert-NoReparsePointInPath -CandidatePath $resolvedPackagePath -RootPath $artifactsRoot

if (![string]::Equals([System.IO.Path]::GetExtension($resolvedPackagePath), ".msix", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "PackagePath file must be a .msix file: $resolvedPackagePath"
}

Write-SmokePlan -ResolvedPackagePath $resolvedPackagePath -ResolvedPackageName $PackageName

if (!$Execute) {
    exit 0
}

Write-Host ""
Write-Host "Executing signed MSIX install smoke..."
Add-AppxPackage -Path $resolvedPackagePath
$installedPackages = @(Get-AppxPackage -Name $PackageName)
if ($installedPackages.Count -eq 0) {
    throw "Installed package was not found by Get-AppxPackage -Name $PackageName."
}

if ($installedPackages.Count -gt 1) {
    throw "Expected one installed package named $PackageName, found $($installedPackages.Count). Refusing to choose a package to remove."
}

$installedPackage = $installedPackages[0]
Write-Host "Installed package:"
Write-Host "  $($installedPackage.PackageFullName)"
Remove-AppxPackage -Package $installedPackage.PackageFullName
Write-Host "Signed MSIX install smoke passed and package was removed."
