[CmdletBinding()]
param(
    [string]$MainPackageUri,
    [string]$OutputPath,
    [string]$AppInstallerUri,
    [string]$ProcessorArchitecture = "x64",
    [switch]$EnableOnLaunchUpdateCheck,
    [int]$HoursBetweenUpdateChecks = 24,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\write-appinstaller.ps1 -MainPackageUri uri [-OutputPath path] [-AppInstallerUri uri] [-ProcessorArchitecture x64] [-EnableOnLaunchUpdateCheck] [-HoursBetweenUpdateChecks 24]"
    Write-Host ""
    Write-Host "Generates a .appinstaller manifest whose MainPackage identity is read from Package.appxmanifest."
    Write-Host "HoursBetweenUpdateChecks must be between 0 and 255 when on-launch update checks are enabled."
    Write-Host "OutputPath must stay inside VoiceInk.Windows\artifacts."
    Write-Host "This script does not publish, install, uninstall, sign, create certificates, import certificates, or trust certificates."
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

if ([string]::IsNullOrWhiteSpace($MainPackageUri)) {
    throw "MainPackageUri is required unless -Help is used."
}

if ($HoursBetweenUpdateChecks -lt 0 -or $HoursBetweenUpdateChecks -gt 255) {
    throw "HoursBetweenUpdateChecks must be between 0 and 255."
}

Assert-AbsoluteUri -Value $MainPackageUri -Name "MainPackageUri"

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"
$manifestPath = Join-Path $windowsRoot "src\VoiceInk.Windows.App\Package.appxmanifest"

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $artifactsRoot "appinstaller\VoiceInk.Windows.appinstaller"
}
elseif (![System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $repoRoot $OutputPath
}

$resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
Assert-PathInside `
    -CandidatePath $resolvedOutputPath `
    -RootPath $artifactsRoot `
    -Message "OutputPath must stay inside VoiceInk.Windows\artifacts."

if ([string]::IsNullOrWhiteSpace($AppInstallerUri)) {
    $AppInstallerUri = ([System.Uri]$resolvedOutputPath).AbsoluteUri
}

Assert-AbsoluteUri -Value $AppInstallerUri -Name "AppInstallerUri"

[xml]$packageManifest = Get-Content -Raw -LiteralPath $manifestPath
$appxNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10"
$namespaceManager = New-Object System.Xml.XmlNamespaceManager($packageManifest.NameTable)
$namespaceManager.AddNamespace("appx", $appxNamespace)
$identity = $packageManifest.SelectSingleNode("/appx:Package/appx:Identity", $namespaceManager)
if ($null -eq $identity) {
    throw "Package.appxmanifest is missing Package/Identity."
}

$packageName = $identity.Name
$packagePublisher = $identity.Publisher
$packageVersion = $identity.Version
if ([string]::IsNullOrWhiteSpace($packageName) -or
    [string]::IsNullOrWhiteSpace($packagePublisher) -or
    [string]::IsNullOrWhiteSpace($packageVersion)) {
    throw "Package.appxmanifest identity must include Name, Publisher, and Version."
}

if ($packageName -ne "VoiceInk.Windows" -or $packagePublisher -ne "CN=VoiceInkOpenSource") {
    throw "Package.appxmanifest identity must remain VoiceInk.Windows / CN=VoiceInkOpenSource."
}

$outputDirectory = [System.IO.Path]::GetDirectoryName($resolvedOutputPath)
if ([string]::IsNullOrWhiteSpace($outputDirectory)) {
    throw "OutputPath must include a directory."
}

New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$appInstallerNamespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2"
$document = New-Object System.Xml.XmlDocument
$declaration = $document.CreateXmlDeclaration("1.0", "utf-8", $null)
$document.AppendChild($declaration) | Out-Null

$rootElement = $document.CreateElement("AppInstaller", $appInstallerNamespace)
$rootElement.SetAttribute("Uri", $AppInstallerUri)
$rootElement.SetAttribute("Version", $packageVersion)
$document.AppendChild($rootElement) | Out-Null

$mainPackageElement = $document.CreateElement("MainPackage", $appInstallerNamespace)
$mainPackageElement.SetAttribute("Name", $packageName)
$mainPackageElement.SetAttribute("Publisher", $packagePublisher)
$mainPackageElement.SetAttribute("Version", $packageVersion)
$mainPackageElement.SetAttribute("ProcessorArchitecture", $ProcessorArchitecture)
$mainPackageElement.SetAttribute("Uri", $MainPackageUri)
$rootElement.AppendChild($mainPackageElement) | Out-Null

if ($EnableOnLaunchUpdateCheck) {
    $updateSettingsElement = $document.CreateElement("UpdateSettings", $appInstallerNamespace)
    $onLaunchElement = $document.CreateElement("OnLaunch", $appInstallerNamespace)
    $onLaunchElement.SetAttribute("HoursBetweenUpdateChecks", $HoursBetweenUpdateChecks.ToString([System.Globalization.CultureInfo]::InvariantCulture))
    $updateSettingsElement.AppendChild($onLaunchElement) | Out-Null
    $rootElement.AppendChild($updateSettingsElement) | Out-Null
}

$document.Save($resolvedOutputPath)

Write-Host "App Installer manifest written:"
Write-Host "  $resolvedOutputPath"
Write-Host "  MainPackage Name: $packageName"
Write-Host "  MainPackage Publisher: $packagePublisher"
Write-Host "  MainPackage Version: $packageVersion"
Write-Host "  MainPackage ProcessorArchitecture: $ProcessorArchitecture"
Write-Host "  MainPackage Uri: $MainPackageUri"
if ($EnableOnLaunchUpdateCheck) {
    Write-Host "  OnLaunch HoursBetweenUpdateChecks: $HoursBetweenUpdateChecks"
}
