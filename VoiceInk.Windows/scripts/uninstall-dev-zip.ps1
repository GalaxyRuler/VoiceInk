[CmdletBinding()]
param(
    [string]$InstallRoot,
    [switch]$Execute,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\uninstall-dev-zip.ps1 [-InstallRoot path] [-Execute]"
    Write-Host ""
    Write-Host "Prints a per-user Dev ZIP uninstall plan by default."
    Write-Host "Pass -Execute to remove the LocalAppData install folder and current-user Start Menu shortcut."
    Write-Host "InstallRoot must stay under LocalAppData."
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

if ($Help) {
    Show-Usage
    exit 0
}

$localAppData = [Environment]::GetFolderPath("LocalApplicationData")
if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path $localAppData "Programs\VoiceInk.Windows"
}
elseif (![System.IO.Path]::IsPathRooted($InstallRoot)) {
    $InstallRoot = Join-Path $localAppData $InstallRoot
}

$resolvedInstallRoot = [System.IO.Path]::GetFullPath($InstallRoot)
$startMenuPrograms = Join-Path ([Environment]::GetFolderPath("StartMenu")) "Programs"
$shortcutPath = Join-Path $startMenuPrograms "VoiceInk for Windows.lnk"

Assert-PathInside -CandidatePath $resolvedInstallRoot -RootPath $localAppData -Message "Refusing to uninstall path outside LocalAppData."

Write-Host "Dev ZIP uninstall plan:"
Write-Host "  Install root: $resolvedInstallRoot"
Write-Host "  Start Menu shortcut: $shortcutPath"
Write-Host ""
Write-Host "Pass -Execute to remove these per-user install artifacts."

if (!$Execute) {
    exit 0
}

if (Test-Path -LiteralPath $shortcutPath) {
    Remove-Item -LiteralPath $shortcutPath -Force
}

if (Test-Path -LiteralPath $resolvedInstallRoot) {
    Assert-PathInside -CandidatePath $resolvedInstallRoot -RootPath $localAppData -Message "Refusing to remove install path outside LocalAppData."
    Remove-Item -LiteralPath $resolvedInstallRoot -Recurse -Force
}

Write-Host "Dev ZIP uninstall completed."
