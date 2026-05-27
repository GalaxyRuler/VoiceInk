[CmdletBinding()]
param(
    [string]$InstallerUrl,
    [string]$InstallerSha256,
    [string]$InstallerPath,
    [string]$OutputRoot,
    [string]$PackageIdentifier = "VoiceInk.VoiceInkWindows",
    [string]$PackageLocale = "en-US",
    [string]$ManifestVersion = "1.10.0",
    [string]$Architecture = "x64",
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\write-winget-manifest.ps1 -InstallerUrl https://example/VoiceInk.msix [-InstallerSha256 sha256 | -InstallerPath artifacts\package.msix]"
    Write-Host ""
    Write-Host "Generates non-mutating WinGet package/default-locale/installer manifests under VoiceInk.Windows\artifacts."
    Write-Host "InstallerSha256 may be supplied by the maintainer, or computed from InstallerPath with Get-FileHash."
    Write-Host "OutputRoot must stay inside VoiceInk.Windows\artifacts."
    Write-Host "This script does not run winget, publish manifests, download installers, install packages, uninstall packages, sign packages, create certificates, import certificates, or trust certificates."
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

function Assert-AppPackageUri {
    param(
        [string]$Value,
        [string]$Name
    )

    $parsedPackageUri = [System.Uri]$Value
    $packageExtension = [System.IO.Path]::GetExtension($parsedPackageUri.AbsolutePath)
    if (![string]::Equals($packageExtension, ".msix", [System.StringComparison]::OrdinalIgnoreCase) -and
        ![string]::Equals($packageExtension, ".msixbundle", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Name must point to a .msix or .msixbundle file: $Value"
    }
}

function Assert-Sha256 {
    param(
        [string]$Value
    )

    if ($Value -notmatch "^[A-Fa-f0-9]{64}$") {
        throw "InstallerSha256 must be a 64-character hexadecimal SHA256 hash."
    }
}

function ConvertTo-YamlScalar {
    param(
        [string]$Value
    )

    if ($Value -match "^[A-Za-z0-9._:/?&=%+-]+$") {
        return $Value
    }

    return "'" + $Value.Replace("'", "''") + "'"
}

function Write-Utf8NoBom {
    param(
        [string]$Path,
        [string]$Content
    )

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content, $utf8NoBom)
}

if ($Help) {
    Show-Usage
    exit 0
}

if ([string]::IsNullOrWhiteSpace($InstallerUrl)) {
    throw "InstallerUrl is required unless -Help is used."
}

Assert-AbsoluteUri -Value $InstallerUrl -Name "InstallerUrl"
Assert-AppPackageUri -Value $InstallerUrl -Name "InstallerUrl"

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"
$manifestPath = Join-Path $windowsRoot "src\VoiceInk.Windows.App\Package.appxmanifest"

if ([string]::IsNullOrWhiteSpace($InstallerSha256)) {
    if ([string]::IsNullOrWhiteSpace($InstallerPath)) {
        throw "InstallerSha256 is required when InstallerPath is not provided."
    }

    $candidateInstallerPath = $InstallerPath
    if (![System.IO.Path]::IsPathRooted($candidateInstallerPath)) {
        $candidateInstallerPath = Join-Path $repoRoot $candidateInstallerPath
    }

    $resolvedInstallerPath = (Resolve-Path -LiteralPath $candidateInstallerPath).Path
    Assert-PathInside `
        -CandidatePath $resolvedInstallerPath `
        -RootPath $artifactsRoot `
        -Message "InstallerPath must stay inside VoiceInk.Windows\artifacts."
    $InstallerSha256 = (Get-FileHash -LiteralPath $resolvedInstallerPath -Algorithm SHA256).Hash.ToUpperInvariant()
}

Assert-Sha256 -Value $InstallerSha256
$normalizedInstallerSha256 = $InstallerSha256.ToUpperInvariant()

[xml]$packageManifest = Get-Content -Raw -LiteralPath $manifestPath
$appxNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10"
$namespaceManager = New-Object System.Xml.XmlNamespaceManager($packageManifest.NameTable)
$namespaceManager.AddNamespace("appx", $appxNamespace)
$identity = $packageManifest.SelectSingleNode("/appx:Package/appx:Identity", $namespaceManager)
if ($null -eq $identity) {
    throw "Package.appxmanifest is missing Package/Identity."
}

$packageVersion = $identity.Version
if ([string]::IsNullOrWhiteSpace($packageVersion)) {
    throw "Package.appxmanifest identity must include Version."
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $artifactsRoot ("winget\{0}\{1}" -f $PackageIdentifier, $packageVersion)
}
elseif (![System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot $OutputRoot
}

$resolvedOutputRoot = [System.IO.Path]::GetFullPath($OutputRoot)
Assert-PathInside `
    -CandidatePath $resolvedOutputRoot `
    -RootPath $artifactsRoot `
    -Message "OutputRoot must stay inside VoiceInk.Windows\artifacts."

New-Item -ItemType Directory -Force -Path $resolvedOutputRoot | Out-Null

$packageManifestFile = Join-Path $resolvedOutputRoot ("{0}.yaml" -f $PackageIdentifier)
$localeManifestFile = Join-Path $resolvedOutputRoot ("{0}.locale.{1}.yaml" -f $PackageIdentifier, $PackageLocale)
$installerManifestFile = Join-Path $resolvedOutputRoot ("{0}.installer.yaml" -f $PackageIdentifier)

$packageYaml = @"
PackageIdentifier: $PackageIdentifier
PackageVersion: $packageVersion
DefaultLocale: $PackageLocale
ManifestType: version
ManifestVersion: $ManifestVersion
"@

$localeYaml = @"
PackageIdentifier: $PackageIdentifier
PackageVersion: $packageVersion
PackageLocale: $PackageLocale
Publisher: VoiceInk Open Source
PublisherUrl: https://github.com/Beingpax/VoiceInk
PackageName: VoiceInk for Windows
PackageUrl: https://github.com/Beingpax/VoiceInk
License: GPL-3.0
LicenseUrl: https://github.com/Beingpax/VoiceInk/blob/main/LICENSE
ShortDescription: Free open-source Windows dictation with local Whisper transcription and optional user-configured providers.
Description: VoiceInk for Windows is a free open-source dictation app with local Whisper transcription, configurable shortcuts, history, dictionary, AI enhancement hooks, and privacy-first local settings.
Tags:
- dictation
- whisper
- transcription
- accessibility
- open-source
ManifestType: defaultLocale
ManifestVersion: $ManifestVersion
"@

$installerYaml = @"
PackageIdentifier: $PackageIdentifier
PackageVersion: $packageVersion
InstallerType: msix
Installers:
- Architecture: $Architecture
  InstallerUrl: $(ConvertTo-YamlScalar -Value $InstallerUrl)
  InstallerSha256: $normalizedInstallerSha256
ManifestType: installer
ManifestVersion: $ManifestVersion
"@

Write-Utf8NoBom -Path $packageManifestFile -Content ($packageYaml.TrimEnd() + [Environment]::NewLine)
Write-Utf8NoBom -Path $localeManifestFile -Content ($localeYaml.TrimEnd() + [Environment]::NewLine)
Write-Utf8NoBom -Path $installerManifestFile -Content ($installerYaml.TrimEnd() + [Environment]::NewLine)

Write-Host "WinGet manifests written:"
Write-Host "  OutputRoot: $resolvedOutputRoot"
Write-Host "  PackageIdentifier: $PackageIdentifier"
Write-Host "  PackageVersion: $packageVersion"
Write-Host "  InstallerType: msix"
Write-Host "  InstallerUrl: $InstallerUrl"
Write-Host "  InstallerSha256: $normalizedInstallerSha256"
Write-Host ""
Write-Host "Optional maintainer validation command:"
Write-Host "  winget validate `"$resolvedOutputRoot`""
Write-Host "This script did not run winget, publish, download, install, uninstall, sign, create certificates, import certificates, or trust certificates."
