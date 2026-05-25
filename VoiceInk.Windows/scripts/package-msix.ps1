[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [Alias("Runtime")]
    [string]$RuntimeIdentifier = "win-x64",
    [string]$OutputRoot,
    [string]$DotNetPath = $env:DOTNET_EXE,
    [Parameter(Mandatory = $false)]
    [string]$PackageCertificateKeyFile,
    [Parameter(Mandatory = $false)]
    [string]$PackageCertificatePassword,
    [switch]$Preflight,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\package-msix.ps1 -PackageCertificateKeyFile path [-PackageCertificatePassword value] [-Configuration Release] [-RuntimeIdentifier win-x64] [-OutputRoot path] [-DotNetPath path]"
    Write-Host "       .\VoiceInk.Windows\scripts\package-msix.ps1 -Preflight [-Configuration Release] [-RuntimeIdentifier win-x64] [-OutputRoot path] [-DotNetPath path]"
    Write-Host ""
    Write-Host "Creates a signed MSIX package under VoiceInk.Windows\artifacts\msix."
    Write-Host "Preflight validates paths, project files, publish properties, and manual smoke commands without requiring a certificate or running dotnet publish."
    Write-Host "Relative DotNetPath, OutputRoot, and PackageCertificateKeyFile values are resolved from the repository root."
    Write-Host "OutputRoot must be inside VoiceInk.Windows\artifacts."
    Write-Host "This script does not create certificates or import certificates into Windows. Provide a maintainer-owned signing certificate."
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

function Write-MsixPublishProperties {
    param(
        [string]$CertificatePath
    )

    Write-Host "MSIX publish properties:"
    Write-Host "  -p:Platform=x64"
    Write-Host "  -p:WindowsPackageType=MSIX"
    Write-Host "  -p:AppxManifest=Package.appxmanifest"
    Write-Host "  -p:GenerateAppxPackageOnBuild=true"
    Write-Host "  -p:AppxBundle=Never"
    Write-Host "  -p:UapAppxPackageBuildMode=SideloadOnly"
    Write-Host "  -p:AppxPackageSigningEnabled=true"
    if ([string]::IsNullOrWhiteSpace($CertificatePath)) {
        Write-Host "  -p:PackageCertificateKeyFile=<maintainer-owned-pfx>"
    }
    else {
        Write-Host "  -p:PackageCertificateKeyFile=`"$CertificatePath`""
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
$appManifest = Join-Path $windowsRoot "src\VoiceInk.Windows.App\Package.appxmanifest"
$dotnet = Resolve-ToolPath -RequestedPath $DotNetPath -RepositoryRoot $repoRoot

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $windowsRoot "artifacts\msix"
}
elseif (![System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot $OutputRoot
}

$windowsArtifactsRoot = Join-Path $windowsRoot "artifacts"
Assert-OutputRootInsideArtifacts -OutputPath $OutputRoot -ArtifactsRoot $windowsArtifactsRoot

$artifactRoot = [System.IO.Path]::GetFullPath($OutputRoot)
$publishRoot = Join-Path $artifactRoot "publish"
Assert-PathInside -CandidatePath $publishRoot -RootPath $artifactRoot

if (!(Test-Path -LiteralPath $appProject -PathType Leaf)) {
    throw "Windows app project was not found: $appProject"
}

if (!(Test-Path -LiteralPath $appManifest -PathType Leaf)) {
    throw "MSIX package manifest was not found: $appManifest"
}

if ($Preflight) {
    Write-Host "MSIX packaging preflight passed:"
    Write-Host "  Repository root: $repoRoot"
    Write-Host "  Windows root: $windowsRoot"
    Write-Host "  App project: $appProject"
    Write-Host "  Package manifest: $appManifest"
    Write-Host "  DotNet path: $dotnet"
    Write-Host "  Artifact root: $artifactRoot"
    Write-Host "  Publish root: $publishRoot"
    Write-Host "  Configuration: $Configuration"
    Write-Host "  Runtime identifier: $RuntimeIdentifier"
    Write-Host ""
    Write-MsixPublishProperties -CertificatePath $null
    Write-Host ""
    Write-Host "Signed package build command shape:"
    Write-Host "  .\VoiceInk.Windows\scripts\package-msix.ps1 -DotNetPath `"$dotnet`" -Configuration $Configuration -RuntimeIdentifier $RuntimeIdentifier -PackageCertificateKeyFile <path-to-maintainer-pfx>"
    Write-Host ""
    Write-Host "Manual smoke commands after a signed package is produced and the signing certificate is trusted on a test machine:"
    Write-Host "  .\VoiceInk.Windows\scripts\test-msix-package.ps1 -PackagePath <path-to-msix>"
    Write-Host "  Add-AppxPackage -Path <path-to-msix>"
    Write-Host "  Get-AppxPackage VoiceInk.Windows"
    Write-Host "  Remove-AppxPackage -Package <package-full-name>"
    Write-Host ""
    Write-Host "Preflight does not run dotnet publish, sign packages, create or import certificates, install packages, uninstall packages, or read certificate passwords."
    exit 0
}

if ([string]::IsNullOrWhiteSpace($PackageCertificateKeyFile)) {
    throw "PackageCertificateKeyFile is required for signed packaging. Provide a maintainer-owned signing certificate path, or run with -Preflight to validate packaging inputs without a certificate."
}

if (![System.IO.Path]::IsPathRooted($PackageCertificateKeyFile)) {
    $PackageCertificateKeyFile = Join-Path $repoRoot $PackageCertificateKeyFile
}

$PackageCertificateKeyFile = (Resolve-Path -LiteralPath $PackageCertificateKeyFile).Path

New-Item -ItemType Directory -Force -Path $artifactRoot | Out-Null
if (Test-Path -LiteralPath $publishRoot) {
    Remove-Item -LiteralPath $publishRoot -Recurse -Force
}

$packagePasswordProperty = if ([string]::IsNullOrEmpty($PackageCertificatePassword)) {
    ""
}
else {
    "-p:PackageCertificatePassword=$PackageCertificatePassword"
}

Write-Host "Publishing signed VoiceInk for Windows MSIX ($Configuration, $RuntimeIdentifier)..."
Write-MsixPublishProperties -CertificatePath $PackageCertificateKeyFile
& $dotnet publish $appProject `
    -c $Configuration `
    -r $RuntimeIdentifier `
    --self-contained true `
    -p:Platform=x64 `
    -p:WindowsPackageType=MSIX `
    -p:AppxManifest=Package.appxmanifest `
    -p:GenerateAppxPackageOnBuild=true `
    -p:AppxBundle=Never `
    -p:UapAppxPackageBuildMode=SideloadOnly `
    -p:AppxPackageSigningEnabled=true `
    -p:PackageCertificateKeyFile="$PackageCertificateKeyFile" `
    $packagePasswordProperty `
    -o $publishRoot

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Write-Host "MSIX artifacts written under:"
Write-Host "  $artifactRoot"
Write-Host ""
Write-Host "Manual smoke commands after trusting the signing certificate on a test machine:"
Write-Host "  Add-AppxPackage -Path <path-to-msix>"
Write-Host "  Get-AppxPackage VoiceInk.Windows"
Write-Host "  Remove-AppxPackage -Package <package-full-name>"
