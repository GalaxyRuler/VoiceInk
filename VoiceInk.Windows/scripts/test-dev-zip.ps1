[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,
    [string]$ScratchRoot,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\test-dev-zip.ps1 -PackagePath path [-ScratchRoot path]"
    Write-Host ""
    Write-Host "Validates an unpackaged dev ZIP or extracted package folder without launching or installing the app."
    Write-Host "PackagePath and ScratchRoot must stay under VoiceInk.Windows\artifacts."
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

function Assert-PathOutside {
    param(
        [string]$CandidatePath,
        [string]$RootPath,
        [string]$Message
    )

    $resolvedRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $resolvedCandidate = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    if ($resolvedCandidate.StartsWith($resolvedRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals($resolvedCandidate, $resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Message Path: $resolvedCandidate Root: $resolvedRoot"
    }
}

function Assert-FileExists {
    param(
        [string]$RootPath,
        [string]$RelativePath
    )

    $path = Join-Path $RootPath $RelativePath
    if (!(Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Expected package file missing: $RelativePath"
    }
}

if ($Help) {
    Show-Usage
    exit 0
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"

if (![System.IO.Path]::IsPathRooted($PackagePath)) {
    $PackagePath = Join-Path $repoRoot $PackagePath
}

$resolvedPackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
Assert-PathInside -CandidatePath $resolvedPackagePath -RootPath $artifactsRoot

if ([string]::IsNullOrWhiteSpace($ScratchRoot)) {
    $ScratchRoot = Join-Path $artifactsRoot "smoke"
}
elseif (![System.IO.Path]::IsPathRooted($ScratchRoot)) {
    $ScratchRoot = Join-Path $repoRoot $ScratchRoot
}

$scratchRootFull = [System.IO.Path]::GetFullPath($ScratchRoot)
Assert-PathInside -CandidatePath $scratchRootFull -RootPath $artifactsRoot

$inspectRoot = $resolvedPackagePath
if ((Get-Item -LiteralPath $resolvedPackagePath) -is [System.IO.FileInfo]) {
    if (![string]::Equals([System.IO.Path]::GetExtension($resolvedPackagePath), ".zip", [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "PackagePath file must be a .zip file: $resolvedPackagePath"
    }

    $extractRoot = Join-Path $scratchRootFull "dev-zip"
    Assert-PathInside -CandidatePath $extractRoot -RootPath $artifactsRoot
    Assert-PathOutside -CandidatePath $resolvedPackagePath -RootPath $extractRoot -Message "Refusing to use a package inside the extraction cleanup root."
    if (Test-Path -LiteralPath $extractRoot) {
        Remove-Item -LiteralPath $extractRoot -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $extractRoot | Out-Null
    Expand-Archive -LiteralPath $resolvedPackagePath -DestinationPath $extractRoot -Force
    $children = Get-ChildItem -LiteralPath $extractRoot -Directory
    $inspectRoot = if ($children.Count -eq 1) { $children[0].FullName } else { $extractRoot }
}

Assert-PathInside -CandidatePath $inspectRoot -RootPath $artifactsRoot
Assert-FileExists -RootPath $inspectRoot -RelativePath "VoiceInk.Windows.App.exe"
Assert-FileExists -RootPath $inspectRoot -RelativePath "VoiceInk.Windows.App.deps.json"
Assert-FileExists -RootPath $inspectRoot -RelativePath "VoiceInk.Windows.App.runtimeconfig.json"
Assert-FileExists -RootPath $inspectRoot -RelativePath "VOICEINK-WINDOWS-README.txt"
Assert-FileExists -RootPath $inspectRoot -RelativePath "LICENSE.txt"
Assert-FileExists -RootPath $inspectRoot -RelativePath "Microsoft.WindowsAppRuntime.Bootstrap.dll"

$readme = Get-Content -Raw -LiteralPath (Join-Path $inspectRoot "VOICEINK-WINDOWS-README.txt")
if ($readme -notmatch "C:\\Models\\ggml-base\.en\.bin") {
    throw "Package README must include the local GGML model smoke-test path."
}

$license = Get-Content -Raw -LiteralPath (Join-Path $inspectRoot "LICENSE.txt")
if ($license -notmatch "GNU GENERAL PUBLIC LICENSE") {
    throw "Package LICENSE.txt must include the GNU GPL license text."
}

Write-Host "Dev ZIP smoke validation passed:"
Write-Host "  $inspectRoot"
