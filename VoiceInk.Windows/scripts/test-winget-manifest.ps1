[CmdletBinding()]
param(
    [string]$ManifestDirectory,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\test-winget-manifest.ps1 -ManifestDirectory VoiceInk.Windows\artifacts\winget\VoiceInk.VoiceInkWindows\<version>"
    Write-Host ""
    Write-Host "Validates generated WinGet manifest files without running winget, publishing, downloading, installing, uninstalling, signing, or changing certificate trust."
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

function Assert-ContainsText {
    param(
        [string]$Text,
        [string]$Needle,
        [string]$Name
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -lt 0) {
        throw "$Name is missing required text: $Needle"
    }
}

function Get-SimpleYamlValue {
    param(
        [string]$Text,
        [string]$Key
    )

    $escapedKey = [System.Text.RegularExpressions.Regex]::Escape($Key)
    $pattern = "(?m)^\s*$escapedKey\s*:\s*(.+?)\s*$"
    $matchResult = [System.Text.RegularExpressions.Regex]::Match($Text, $pattern)
    if (!$matchResult.Success) {
        return $null
    }

    return $matchResult.Groups[1].Value.Trim().Trim("'")
}

if ($Help) {
    Show-Usage
    exit 0
}

if ([string]::IsNullOrWhiteSpace($ManifestDirectory)) {
    throw "ManifestDirectory is required unless -Help is used."
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"
$resolvedManifestDirectory = (Resolve-Path -LiteralPath $ManifestDirectory).Path
Assert-PathInside `
    -CandidatePath $resolvedManifestDirectory `
    -RootPath $artifactsRoot `
    -Message "Refusing to inspect path outside artifact root."

$packageFile = Get-ChildItem -LiteralPath $resolvedManifestDirectory -File -Filter "*.yaml" |
    Where-Object { $_.Name -notmatch "\.locale\." -and $_.Name -notmatch "\.installer\.yaml$" } |
    Select-Object -First 1
$localeFile = Get-ChildItem -LiteralPath $resolvedManifestDirectory -File -Filter "*.locale.*.yaml" | Select-Object -First 1
$installerFile = Get-ChildItem -LiteralPath $resolvedManifestDirectory -File -Filter "*.installer.yaml" | Select-Object -First 1

if ($null -eq $packageFile -or $null -eq $localeFile -or $null -eq $installerFile) {
    throw "ManifestDirectory must contain package, default-locale, and installer WinGet YAML files."
}

$packageText = Get-Content -Raw -LiteralPath $packageFile.FullName
$localeText = Get-Content -Raw -LiteralPath $localeFile.FullName
$installerText = Get-Content -Raw -LiteralPath $installerFile.FullName

Assert-ContainsText -Text $packageText -Needle "PackageIdentifier:" -Name $packageFile.Name
Assert-ContainsText -Text $packageText -Needle "PackageVersion:" -Name $packageFile.Name
Assert-ContainsText -Text $packageText -Needle "DefaultLocale:" -Name $packageFile.Name
Assert-ContainsText -Text $packageText -Needle "ManifestType: version" -Name $packageFile.Name
Assert-ContainsText -Text $packageText -Needle "ManifestVersion:" -Name $packageFile.Name
Assert-ContainsText -Text $localeText -Needle "PackageName: VoiceInk for Windows" -Name $localeFile.Name
Assert-ContainsText -Text $localeText -Needle "License: GPL-3.0" -Name $localeFile.Name
Assert-ContainsText -Text $localeText -Needle "ManifestType: defaultLocale" -Name $localeFile.Name
Assert-ContainsText -Text $installerText -Needle "InstallerType: msix" -Name $installerFile.Name
Assert-ContainsText -Text $installerText -Needle "InstallerUrl:" -Name $installerFile.Name
Assert-ContainsText -Text $installerText -Needle "InstallerSha256:" -Name $installerFile.Name
Assert-ContainsText -Text $installerText -Needle "ManifestType: installer" -Name $installerFile.Name

$packageIdentifier = Get-SimpleYamlValue -Text $packageText -Key "PackageIdentifier"
$packageVersion = Get-SimpleYamlValue -Text $packageText -Key "PackageVersion"
$installerIdentifier = Get-SimpleYamlValue -Text $installerText -Key "PackageIdentifier"
$installerVersion = Get-SimpleYamlValue -Text $installerText -Key "PackageVersion"
$installerUrl = Get-SimpleYamlValue -Text $installerText -Key "InstallerUrl"
$installerSha256 = Get-SimpleYamlValue -Text $installerText -Key "InstallerSha256"

if ($packageIdentifier -ne $installerIdentifier) {
    throw "PackageIdentifier must match between package and installer manifests."
}

if ($packageVersion -ne $installerVersion) {
    throw "PackageVersion must match between package and installer manifests."
}

if ($installerUrl -notmatch "^https?://.+\.(msix|msixbundle)$" -and $installerUrl -notmatch "^file:///.+\.(msix|msixbundle)$") {
    throw "InstallerUrl must point to an absolute .msix or .msixbundle URI."
}

if ($installerSha256 -notmatch "^[A-Fa-f0-9]{64}$") {
    throw "InstallerSha256 must be a 64-character hexadecimal SHA256 hash."
}

$combinedText = $packageText + "`n" + $localeText + "`n" + $installerText
if ($combinedText.IndexOf("trial", [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or
    $combinedText.IndexOf("purchase", [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
    throw "WinGet manifests must not contain commercial trial or purchase language."
}

Write-Host "WinGet manifest validation passed:"
Write-Host "  ManifestDirectory: $resolvedManifestDirectory"
Write-Host "  PackageIdentifier: $packageIdentifier"
Write-Host "  PackageVersion: $packageVersion"
Write-Host "  InstallerType: msix"
Write-Host "  InstallerUrl: $installerUrl"
Write-Host "  InstallerSha256: $installerSha256"
Write-Host ""
Write-Host "Optional maintainer validation command:"
Write-Host "  winget validate `"$resolvedManifestDirectory`""
Write-Host "This script did not run winget, publish, download, install, uninstall, sign, create certificates, import certificates, or trust certificates."
