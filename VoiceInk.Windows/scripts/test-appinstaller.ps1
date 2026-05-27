[CmdletBinding()]
param(
    [string]$AppInstallerPath,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\test-appinstaller.ps1 -AppInstallerPath path"
    Write-Host ""
    Write-Host "Validates .appinstaller schema identity against Package.appxmanifest without publishing, launching, installing, signing, or trusting certificates."
    Write-Host "AppInstallerPath must stay under VoiceInk.Windows\artifacts."
}

function Assert-PathInside {
    param(
        [string]$CandidatePath,
        [string]$RootPath
    )

    $resolvedRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $resolvedCandidate = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    if (!$resolvedCandidate.StartsWith($resolvedRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase) -and
        ![string]::Equals($resolvedCandidate, $resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to inspect path outside artifact root. Path: $resolvedCandidate Root: $resolvedRoot"
    }
}

function Assert-AbsoluteUri {
    param(
        [string]$Value,
        [string]$Name
    )

    $parsedUri = $null
    if (![System.Uri]::TryCreate($Value, [System.UriKind]::Absolute, [ref]$parsedUri)) {
        throw "$Name must be an absolute URI: $Value"
    }
}

if ($Help) {
    Show-Usage
    exit 0
}

if ([string]::IsNullOrWhiteSpace($AppInstallerPath)) {
    throw "AppInstallerPath is required unless -Help is used."
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"
$packageManifestPath = Join-Path $windowsRoot "src\VoiceInk.Windows.App\Package.appxmanifest"

if (![System.IO.Path]::IsPathRooted($AppInstallerPath)) {
    $AppInstallerPath = Join-Path $repoRoot $AppInstallerPath
}

$resolvedAppInstallerPath = (Resolve-Path -LiteralPath $AppInstallerPath).Path
Assert-PathInside -CandidatePath $resolvedAppInstallerPath -RootPath $artifactsRoot

if (![string]::Equals([System.IO.Path]::GetExtension($resolvedAppInstallerPath), ".appinstaller", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "AppInstallerPath file must be a .appinstaller file: $resolvedAppInstallerPath"
}

[xml]$packageManifest = Get-Content -Raw -LiteralPath $packageManifestPath
$appxNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10"
$packageNamespaceManager = New-Object System.Xml.XmlNamespaceManager($packageManifest.NameTable)
$packageNamespaceManager.AddNamespace("appx", $appxNamespace)
$identity = $packageManifest.SelectSingleNode("/appx:Package/appx:Identity", $packageNamespaceManager)
if ($null -eq $identity) {
    throw "Package.appxmanifest is missing Package/Identity."
}

[xml]$appInstaller = Get-Content -Raw -LiteralPath $resolvedAppInstallerPath
$appInstallerNamespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2"
$appInstallerNamespaceManager = New-Object System.Xml.XmlNamespaceManager($appInstaller.NameTable)
$appInstallerNamespaceManager.AddNamespace("ai", $appInstallerNamespace)

$root = $appInstaller.SelectSingleNode("/ai:AppInstaller", $appInstallerNamespaceManager)
if ($null -eq $root) {
    throw "App Installer manifest must use AppInstaller root with namespace $appInstallerNamespace."
}

$mainPackage = $appInstaller.SelectSingleNode("/ai:AppInstaller/ai:MainPackage", $appInstallerNamespaceManager)
if ($null -eq $mainPackage) {
    throw "App Installer manifest must include MainPackage."
}

if ($mainPackage.Name -ne $identity.Name -or
    $mainPackage.Publisher -ne $identity.Publisher -or
    $mainPackage.Version -ne $identity.Version) {
    throw "MainPackage Name, Publisher, and Version must match Package.appxmanifest."
}

if ([string]::IsNullOrWhiteSpace($mainPackage.ProcessorArchitecture)) {
    throw "MainPackage ProcessorArchitecture is required."
}

Assert-AbsoluteUri -Value $root.Uri -Name "AppInstaller Uri"
Assert-AbsoluteUri -Value $mainPackage.Uri -Name "MainPackage Uri"

Write-Host "App Installer manifest validation passed:"
Write-Host "  $resolvedAppInstallerPath"
Write-Host "  Name: $($mainPackage.Name)"
Write-Host "  Publisher: $($mainPackage.Publisher)"
Write-Host "  Version: $($mainPackage.Version)"
Write-Host "  ProcessorArchitecture: $($mainPackage.ProcessorArchitecture)"
Write-Host "  Uri: $($mainPackage.Uri)"
