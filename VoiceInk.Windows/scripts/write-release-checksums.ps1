[CmdletBinding()]
param(
    [string[]]$ArtifactPath,
    [string]$OutputPath,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\write-release-checksums.ps1 -ArtifactPath <artifact1>,<artifact2> [-OutputPath VoiceInk.Windows\artifacts\release\SHA256SUMS.txt]"
    Write-Host ""
    Write-Host "Writes a deterministic SHA256SUMS.txt manifest for release artifacts."
    Write-Host "ArtifactPath and OutputPath must stay inside VoiceInk.Windows\artifacts."
    Write-Host "This script does not install packages, uninstall packages, run winget, publish artifacts, sign packages, create certificates, import certificates, or trust certificates."
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

function Get-ArtifactRelativePath {
    param(
        [string]$ResolvedArtifactPath,
        [string]$ArtifactsRoot
    )

    $artifactRootWithSeparator = [System.IO.Path]::GetFullPath($ArtifactsRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar) + [System.IO.Path]::DirectorySeparatorChar
    $relativePath = $ResolvedArtifactPath.Substring($artifactRootWithSeparator.Length)
    return $relativePath.Replace([System.IO.Path]::DirectorySeparatorChar, "/")
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

$artifactInputPaths = @($ArtifactPath)
if ($null -eq $ArtifactPath -or $artifactInputPaths.Count -eq 0) {
    throw "ArtifactPath is required unless -Help is used."
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $artifactsRoot "release\SHA256SUMS.txt"
}
elseif (![System.IO.Path]::IsPathRooted($OutputPath)) {
    $OutputPath = Join-Path $repoRoot $OutputPath
}

$resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
Assert-PathInside `
    -CandidatePath $resolvedOutputPath `
    -RootPath $artifactsRoot `
    -Message "OutputPath must stay inside VoiceInk.Windows\artifacts."

$checksumRows = New-Object System.Collections.Generic.List[string]
$seenArtifacts = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::OrdinalIgnoreCase)

foreach ($artifactInputPath in $artifactInputPaths) {
    if ([string]::IsNullOrWhiteSpace($artifactInputPath)) {
        continue
    }

    $candidateArtifactPath = $artifactInputPath
    if (![System.IO.Path]::IsPathRooted($candidateArtifactPath)) {
        $candidateArtifactPath = Join-Path $repoRoot $candidateArtifactPath
    }

    $resolvedArtifactPath = (Resolve-Path -LiteralPath $candidateArtifactPath).Path
    if ((Get-Item -LiteralPath $resolvedArtifactPath).PSIsContainer) {
        throw "ArtifactPath must point to a file, not a directory: $resolvedArtifactPath"
    }

    Assert-PathInside `
        -CandidatePath $resolvedArtifactPath `
        -RootPath $artifactsRoot `
        -Message "ArtifactPath must stay inside VoiceInk.Windows\artifacts."

    if (!$seenArtifacts.Add($resolvedArtifactPath)) {
        continue
    }

    $hash = (Get-FileHash -LiteralPath $resolvedArtifactPath -Algorithm SHA256).Hash.ToUpperInvariant()
    $relativePath = Get-ArtifactRelativePath -ResolvedArtifactPath $resolvedArtifactPath -ArtifactsRoot $artifactsRoot
    $checksumRows.Add(("{0} *{1}" -f $hash, $relativePath))
}

if ($checksumRows.Count -eq 0) {
    throw "No artifact files were provided."
}

$sortedRows = @($checksumRows | Sort-Object)
$outputDirectory = [System.IO.Path]::GetDirectoryName($resolvedOutputPath)
if ([string]::IsNullOrWhiteSpace($outputDirectory)) {
    throw "OutputPath must include a directory."
}

New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
Write-Utf8NoBom -Path $resolvedOutputPath -Content (($sortedRows -join [Environment]::NewLine) + [Environment]::NewLine)

Write-Host "Release checksums written:"
Write-Host "  $resolvedOutputPath"
Write-Host "  Artifact count: $($sortedRows.Count)"
Write-Host "  Format: SHA256 *artifact-relative-path"
Write-Host "This script did not install, uninstall, run winget, publish, sign, create certificates, import certificates, or trust certificates."
