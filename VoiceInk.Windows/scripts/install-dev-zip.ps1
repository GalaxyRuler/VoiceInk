[CmdletBinding()]
param(
    [string]$PackagePath,
    [string]$InstallRoot,
    [switch]$Execute,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\install-dev-zip.ps1 -PackagePath path [-InstallRoot path] [-Execute]"
    Write-Host ""
    Write-Host "Prints a per-user Dev ZIP install plan by default."
    Write-Host "Pass -Execute to extract the package under LocalAppData and create a current-user Start Menu shortcut."
    Write-Host "PackagePath must be a .zip under VoiceInk.Windows\artifacts."
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
                throw "Refusing to use reparse-point path for Dev ZIP install. Path: $currentPath"
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

function Write-InstallPlan {
    param(
        [string]$ResolvedPackagePath,
        [string]$ResolvedInstallRoot,
        [string]$ShortcutPath
    )

    Write-Host "Dev ZIP install plan:"
    Write-Host "  Package: $ResolvedPackagePath"
    Write-Host "  Install root: $ResolvedInstallRoot"
    Write-Host "  Start Menu shortcut: $ShortcutPath"
    Write-Host ""
    Write-Host "Pass -Execute to extract the package, replace the per-user install folder, and create the shortcut."
}

if ($Help) {
    Show-Usage
    exit 0
}

if ([string]::IsNullOrWhiteSpace($PackagePath)) {
    throw "PackagePath is required unless -Help is used."
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"
$localAppData = [Environment]::GetFolderPath("LocalApplicationData")

if ([string]::IsNullOrWhiteSpace($InstallRoot)) {
    $InstallRoot = Join-Path $localAppData "Programs\VoiceInk.Windows"
}
elseif (![System.IO.Path]::IsPathRooted($InstallRoot)) {
    $InstallRoot = Join-Path $localAppData $InstallRoot
}

if (![System.IO.Path]::IsPathRooted($PackagePath)) {
    $PackagePath = Join-Path $repoRoot $PackagePath
}

$resolvedPackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
$resolvedInstallRoot = [System.IO.Path]::GetFullPath($InstallRoot)
$startMenuPrograms = Join-Path ([Environment]::GetFolderPath("StartMenu")) "Programs"
$shortcutPath = Join-Path $startMenuPrograms "VoiceInk for Windows.lnk"
$temporaryExtractRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("VoiceInk.Windows.Install." + [Guid]::NewGuid().ToString("N"))

Assert-PathInside -CandidatePath $resolvedPackagePath -RootPath $artifactsRoot -Message "Refusing to install from a package outside artifact root."
Assert-PathInside -CandidatePath $resolvedInstallRoot -RootPath $localAppData -Message "InstallRoot must stay under LocalAppData."
Assert-NoReparsePointInPath -CandidatePath $resolvedPackagePath -RootPath $artifactsRoot
Assert-NoReparsePointInPath -CandidatePath $resolvedInstallRoot -RootPath $localAppData

if (![string]::Equals([System.IO.Path]::GetExtension($resolvedPackagePath), ".zip", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "PackagePath file must be a .zip file: $resolvedPackagePath"
}

Write-InstallPlan -ResolvedPackagePath $resolvedPackagePath -ResolvedInstallRoot $resolvedInstallRoot -ShortcutPath $shortcutPath

if (!$Execute) {
    exit 0
}

try {
    New-Item -ItemType Directory -Force -Path $temporaryExtractRoot | Out-Null
    Expand-Archive -LiteralPath $resolvedPackagePath -DestinationPath $temporaryExtractRoot -Force
    $appExe = @(Get-ChildItem -LiteralPath $temporaryExtractRoot -Recurse -File -Filter "VoiceInk.Windows.App.exe")
    if ($appExe.Count -ne 1) {
        throw "Expected one VoiceInk.Windows.App.exe in the package, found $($appExe.Count)."
    }

    if (Test-Path -LiteralPath $resolvedInstallRoot) {
        Assert-PathInside -CandidatePath $resolvedInstallRoot -RootPath $localAppData -Message "Refusing to replace install path outside LocalAppData."
        Remove-Item -LiteralPath $resolvedInstallRoot -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $resolvedInstallRoot | Out-Null
    Copy-Item -Path (Join-Path $appExe[0].DirectoryName "*") -Destination $resolvedInstallRoot -Recurse -Force

    $installedExe = Join-Path $resolvedInstallRoot "VoiceInk.Windows.App.exe"
    if (!(Test-Path -LiteralPath $installedExe -PathType Leaf)) {
        throw "Installed executable was not found: $installedExe"
    }

    New-Item -ItemType Directory -Force -Path $startMenuPrograms | Out-Null
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $installedExe
    $shortcut.WorkingDirectory = $resolvedInstallRoot
    $shortcut.Description = "VoiceInk for Windows"
    $shortcut.Save()

    Write-Host "Dev ZIP install completed:"
    Write-Host "  $installedExe"
}
finally {
    if (Test-Path -LiteralPath $temporaryExtractRoot) {
        Remove-Item -LiteralPath $temporaryExtractRoot -Recurse -Force
    }
}
