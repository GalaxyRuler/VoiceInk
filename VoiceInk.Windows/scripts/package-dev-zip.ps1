[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [Alias("Runtime")]
    [string]$RuntimeIdentifier = "win-x64",
    [string]$OutputRoot,
    [string]$DotNetPath = $env:DOTNET_EXE,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\package-dev-zip.ps1 [-Configuration Release] [-RuntimeIdentifier win-x64] [-OutputRoot path] [-DotNetPath path]"
    Write-Host ""
    Write-Host "Creates an unpackaged, self-contained Windows dev ZIP under VoiceInk.Windows\artifacts\dev-zip."
    Write-Host "Relative DotNetPath and OutputRoot values are resolved from the repository root."
    Write-Host "OutputRoot must stay under VoiceInk.Windows\artifacts."
}

function Resolve-ToolPath {
    param(
        [string]$RequestedPath,
        [string]$RepositoryRoot
    )

    if (![string]::IsNullOrWhiteSpace($RequestedPath)) {
        if ($RequestedPath -eq "dotnet") {
            return "dotnet"
        }

        $candidatePath = $RequestedPath
        if (![System.IO.Path]::IsPathRooted($candidatePath)) {
            $candidatePath = Join-Path $RepositoryRoot $candidatePath
        }

        return (Resolve-Path -LiteralPath $candidatePath).Path
    }

    $repoLocalDotNet = Join-Path (Split-Path -Parent $RepositoryRoot) ".dotnet-sdk-10\dotnet.exe"
    if (Test-Path -LiteralPath $repoLocalDotNet) {
        return (Resolve-Path -LiteralPath $repoLocalDotNet).Path
    }

    return "dotnet"
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
        throw "Refusing to modify path outside artifact root. Path: $resolvedCandidate Root: $resolvedRoot"
    }
}

function Assert-OutputRootInsideArtifacts {
    param(
        [string]$OutputPath,
        [string]$ArtifactsRoot
    )

    $resolvedArtifactsRoot = [System.IO.Path]::GetFullPath($ArtifactsRoot).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $resolvedOutputPath = [System.IO.Path]::GetFullPath($OutputPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)

    if (![string]::Equals($resolvedOutputPath, $resolvedArtifactsRoot, [System.StringComparison]::OrdinalIgnoreCase) -and
        !$resolvedOutputPath.StartsWith($resolvedArtifactsRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "OutputRoot must be inside VoiceInk.Windows\artifacts. OutputRoot: $resolvedOutputPath Artifacts: $resolvedArtifactsRoot"
    }
}

if ($Help) {
    Show-Usage
    exit 0
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$appProject = Join-Path $windowsRoot "src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj"
$dotnet = Resolve-ToolPath -RequestedPath $DotNetPath -RepositoryRoot $repoRoot

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $windowsRoot "artifacts\dev-zip"
}
elseif (![System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot $OutputRoot
}

$windowsArtifactsRoot = Join-Path $windowsRoot "artifacts"
Assert-OutputRootInsideArtifacts -OutputPath $OutputRoot -ArtifactsRoot $windowsArtifactsRoot

$artifactRoot = [System.IO.Path]::GetFullPath($OutputRoot)
$publishRoot = Join-Path $artifactRoot "publish"
$packageRoot = Join-Path $artifactRoot "package"
$packageName = "VoiceInk-Windows-dev-$RuntimeIdentifier"
$packageDirectory = Join-Path $packageRoot $packageName
$zipPath = Join-Path $artifactRoot "$packageName.zip"

Assert-PathInside -CandidatePath $publishRoot -RootPath $artifactRoot
Assert-PathInside -CandidatePath $packageRoot -RootPath $artifactRoot
Assert-PathInside -CandidatePath $packageDirectory -RootPath $artifactRoot
Assert-PathInside -CandidatePath $zipPath -RootPath $artifactRoot

New-Item -ItemType Directory -Force -Path $artifactRoot | Out-Null

foreach ($pathToClean in @($publishRoot, $packageRoot, $zipPath)) {
    Assert-PathInside -CandidatePath $pathToClean -RootPath $artifactRoot
    if (Test-Path -LiteralPath $pathToClean) {
        Remove-Item -LiteralPath $pathToClean -Recurse -Force
    }
}

Write-Host "Publishing VoiceInk for Windows ($Configuration, $RuntimeIdentifier)..."
& $dotnet publish $appProject `
    -c $Configuration `
    -r $RuntimeIdentifier `
    --self-contained true `
    -p:Platform=x64 `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=true `
    -p:PublishSingleFile=false `
    -o $publishRoot

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

$publishedExe = Join-Path $publishRoot "VoiceInk.Windows.App.exe"
if (!(Test-Path -LiteralPath $publishedExe)) {
    throw "Published app executable was not found: $publishedExe"
}

New-Item -ItemType Directory -Force -Path $packageDirectory | Out-Null
Copy-Item -Path (Join-Path $publishRoot "*") -Destination $packageDirectory -Recurse -Force

$readme = @"
VoiceInk for Windows dev ZIP
============================

Launch:
  VoiceInk.Windows.App.exe

First-run setup asks for a whisper.cpp-compatible GGML model such as:
  C:\Models\ggml-base.en.bin

App data:
  %LocalAppData%\VoiceInk.Windows

Notes:
  - This is an unpackaged developer ZIP for testing the open-source Windows fork.
  - It includes .NET and Windows App SDK dependencies next to the app where supported by self-contained deployment.
  - It is not an installer and does not register Start Menu shortcuts, uninstall entries, file associations, or auto-update behavior.
  - MSIX or installer packaging remains a separate release step.
"@

Set-Content -LiteralPath (Join-Path $packageDirectory "VOICEINK-WINDOWS-README.txt") -Value $readme -Encoding UTF8

Write-Host "Creating ZIP package..."
Compress-Archive -Path (Join-Path $packageDirectory "*") -DestinationPath $zipPath -Force

Write-Host "Package created:"
Write-Host "  $zipPath"
