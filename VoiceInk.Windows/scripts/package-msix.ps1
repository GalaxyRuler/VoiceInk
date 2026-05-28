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
    [Parameter(Mandatory = $false)]
    [string]$PackageCertificateThumbprint,
    [switch]$UseLocalMachineCertificateStore,
    [string]$TimestampServerUrl = "https://timestamp.acs.microsoft.com",
    [string]$TimestampDigestAlgorithm = "SHA256",
    [string]$MakeAppxPath,
    [string]$SignToolPath,
    [string]$WindowsAppRuntimePackageRoot,
    [switch]$Preflight,
    [switch]$ValidateAfterBuild,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\package-msix.ps1 -PackageCertificateKeyFile path [-PackageCertificatePassword value] [-TimestampServerUrl uri] [-TimestampDigestAlgorithm SHA256] [-ValidateAfterBuild] [-Configuration Release] [-RuntimeIdentifier win-x64] [-OutputRoot path] [-DotNetPath path]"
    Write-Host "       .\VoiceInk.Windows\scripts\package-msix.ps1 -PackageCertificateThumbprint thumbprint [-UseLocalMachineCertificateStore] [-TimestampServerUrl uri] [-TimestampDigestAlgorithm SHA256] [-ValidateAfterBuild] [-Configuration Release] [-RuntimeIdentifier win-x64] [-OutputRoot path] [-DotNetPath path]"
    Write-Host "       .\VoiceInk.Windows\scripts\package-msix.ps1 -Preflight [-TimestampServerUrl uri] [-TimestampDigestAlgorithm SHA256] [-Configuration Release] [-RuntimeIdentifier win-x64] [-OutputRoot path] [-DotNetPath path]"
    Write-Host ""
    Write-Host "Creates a signed MSIX package under VoiceInk.Windows\artifacts\msix using the runner-proven publish, MakeAppx, and SignTool path."
    Write-Host "Preflight validates paths, project files, packaging tools, staging rules, and manual smoke commands without requiring a certificate or running dotnet publish."
    Write-Host "Relative DotNetPath, OutputRoot, PackageCertificateKeyFile, MakeAppxPath, SignToolPath, and WindowsAppRuntimePackageRoot values are resolved from the repository root."
    Write-Host "OutputRoot must be inside VoiceInk.Windows\artifacts."
    Write-Host "TimestampServerUrl defaults to https://timestamp.acs.microsoft.com and TimestampDigestAlgorithm defaults to SHA256 for timestamped package signatures."
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

function Resolve-OptionalPath {
    param(
        [string]$RequestedPath,
        [string]$RepositoryRoot
    )

    if ([string]::IsNullOrWhiteSpace($RequestedPath)) {
        return $null
    }

    $candidatePath = $RequestedPath
    if (![System.IO.Path]::IsPathRooted($candidatePath)) {
        $candidatePath = Join-Path $RepositoryRoot $candidatePath
    }

    return (Resolve-Path -LiteralPath $candidatePath).Path
}

function Find-WindowsKitTool {
    param(
        [string]$ToolName,
        [string]$RequestedPath,
        [string]$RepositoryRoot
    )

    $resolvedPath = Resolve-OptionalPath -RequestedPath $RequestedPath -RepositoryRoot $RepositoryRoot
    if ($null -ne $resolvedPath) {
        return $resolvedPath
    }

    $programFilesX86 = ${env:ProgramFiles(x86)}
    if ([string]::IsNullOrWhiteSpace($programFilesX86)) {
        $programFilesX86 = Join-Path $env:SystemDrive "Program Files (x86)"
    }

    $windowsKitsRoot = Join-Path $programFilesX86 "Windows Kits\10\bin"
    if (Test-Path -LiteralPath $windowsKitsRoot) {
        $tool = Get-ChildItem -LiteralPath $windowsKitsRoot -Recurse -File -Filter $ToolName -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "\\x64\\$([regex]::Escape($ToolName))$" } |
            Sort-Object FullName -Descending |
            Select-Object -First 1

        if ($null -ne $tool) {
            return $tool.FullName
        }
    }

    return $ToolName
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

function Assert-AbsoluteUri {
    param(
        [string]$Value,
        [string]$Name
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "$Name cannot be blank."
    }

    $parsedUri = $null
    if (![System.Uri]::TryCreate($Value, [System.UriKind]::Absolute, [ref]$parsedUri) -or [string]::IsNullOrWhiteSpace($parsedUri.Scheme)) {
        throw "$Name must be an absolute URI: $Value"
    }
}

function Write-MsixPublishProperties {
    Write-Host "MSIX publish/staging properties:"
    Write-Host "  dotnet publish"
    Write-Host "  -p:Platform=x64"
    Write-Host "  -p:WindowsPackageType=None"
    Write-Host "  -p:WindowsAppSDKSelfContained=false"
    Write-Host "  -p:WindowsAppSdkBootstrapInitialize=false"
    Write-Host "  -p:WindowsAppSdkDeploymentManagerInitialize=false"
    Write-Host "  -p:PublishSingleFile=false"
    Write-Host "  stage AppxManifest.xml from Package.appxmanifest"
    Write-Host "  stage Assets, *.xbf, resources.pri, VoiceInk.Windows.App.pri"
    Write-Host "  stage Microsoft.UI.pri, Microsoft.UI.Xaml.Controls.pri, Microsoft.WindowsAppRuntime.pri"
    Write-Host "  MakeAppx pack /d <publish-root> /p <msix>"
    Write-Host "  SignTool sign /fd SHA256 /tr `"$TimestampServerUrl`" /td $TimestampDigestAlgorithm"
}

function Find-NewestFile {
    param(
        [string]$SearchRoot,
        [string]$Filter,
        [string]$Description
    )

    $file = Get-ChildItem -LiteralPath $SearchRoot -Recurse -File -Filter $Filter -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1

    if ($null -eq $file) {
        throw "Could not find $Description under $SearchRoot."
    }

    return $file.FullName
}

function Copy-RequiredFile {
    param(
        [string]$SourcePath,
        [string]$DestinationPath,
        [string]$Description
    )

    if (!(Test-Path -LiteralPath $SourcePath -PathType Leaf)) {
        throw "Required $Description was not found: $SourcePath"
    }

    Copy-Item -LiteralPath $SourcePath -Destination $DestinationPath -Force
}

function Copy-XamlBinaryFiles {
    param(
        [string]$AppProjectDirectory,
        [string]$PublishRoot
    )

    $expectedXbfFiles = @(
        "App.xbf",
        "MainWindow.xbf",
        "FloatingRecorderWindow.xbf",
        "HistoryWindow.xbf",
        "OcrRegionPickerWindow.xbf"
    )

    foreach ($xbfFileName in $expectedXbfFiles) {
        $sourcePath = Find-NewestFile -SearchRoot (Join-Path $AppProjectDirectory "obj") -Filter $xbfFileName -Description $xbfFileName
        Copy-Item -LiteralPath $sourcePath -Destination (Join-Path $PublishRoot $xbfFileName) -Force
    }
}

function Find-WindowsAppRuntimeRoot {
    param(
        [string]$RequestedRoot,
        [string]$RepositoryRoot
    )

    $resolvedRoot = Resolve-OptionalPath -RequestedPath $RequestedRoot -RepositoryRoot $RepositoryRoot
    if ($null -ne $resolvedRoot) {
        return $resolvedRoot
    }

    $windowsAppsRoot = Join-Path $env:ProgramFiles "WindowsApps"
    if (Test-Path -LiteralPath $windowsAppsRoot) {
        $runtimeRoot = Get-ChildItem -LiteralPath $windowsAppsRoot -Directory -Filter "Microsoft.WindowsAppRuntime.1.8_*_x64__8wekyb3d8bbwe" -ErrorAction SilentlyContinue |
            Sort-Object Name -Descending |
            Select-Object -First 1

        if ($null -ne $runtimeRoot) {
            return $runtimeRoot.FullName
        }
    }

    throw "Could not find Microsoft.WindowsAppRuntime.1.8 x64 package root. Install Windows App Runtime 1.8, or pass -WindowsAppRuntimePackageRoot."
}

function Copy-WindowsAppRuntimePriFiles {
    param(
        [string]$RuntimeRoot,
        [string]$PublishRoot
    )

    $runtimePriFiles = @(
        "Microsoft.UI.pri",
        "Microsoft.UI.Xaml.Controls.pri",
        "Microsoft.WindowsAppRuntime.pri"
    )

    foreach ($priFileName in $runtimePriFiles) {
        $sourcePath = Find-NewestFile -SearchRoot $RuntimeRoot -Filter $priFileName -Description $priFileName
        Copy-Item -LiteralPath $sourcePath -Destination (Join-Path $PublishRoot $priFileName) -Force
    }
}

function Invoke-AndCheck {
    param(
        [scriptblock]$Command,
        [string]$FailureMessage
    )

    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$FailureMessage Exit code: $LASTEXITCODE."
    }
}

if ($Help) {
    Show-Usage
    exit 0
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$appProjectDirectory = Join-Path $windowsRoot "src\VoiceInk.Windows.App"
$appProject = Join-Path $appProjectDirectory "VoiceInk.Windows.App.csproj"
$appManifest = Join-Path $appProjectDirectory "Package.appxmanifest"
$assetsRoot = Join-Path $appProjectDirectory "Assets"
$dotnet = Resolve-ToolPath -RequestedPath $DotNetPath -RepositoryRoot $repoRoot
$makeAppx = Find-WindowsKitTool -ToolName "makeappx.exe" -RequestedPath $MakeAppxPath -RepositoryRoot $repoRoot
$signTool = Find-WindowsKitTool -ToolName "signtool.exe" -RequestedPath $SignToolPath -RepositoryRoot $repoRoot

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $windowsRoot "artifacts\msix"
}
elseif (![System.IO.Path]::IsPathRooted($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot $OutputRoot
}

$windowsArtifactsRoot = Join-Path $windowsRoot "artifacts"
Assert-OutputRootInsideArtifacts -OutputPath $OutputRoot -ArtifactsRoot $windowsArtifactsRoot
Assert-AbsoluteUri -Value $TimestampServerUrl -Name "TimestampServerUrl"
if ([string]::IsNullOrWhiteSpace($TimestampDigestAlgorithm)) {
    throw "TimestampDigestAlgorithm cannot be blank."
}

$artifactRoot = [System.IO.Path]::GetFullPath($OutputRoot)
$publishRoot = Join-Path $artifactRoot "publish"
$packagePath = Join-Path $artifactRoot "VoiceInk.Windows.signed.msix"
Assert-PathInside -CandidatePath $publishRoot -RootPath $artifactRoot
Assert-PathInside -CandidatePath $packagePath -RootPath $artifactRoot

if (!(Test-Path -LiteralPath $appProject -PathType Leaf)) {
    throw "Windows app project was not found: $appProject"
}

if (!(Test-Path -LiteralPath $appManifest -PathType Leaf)) {
    throw "MSIX package manifest was not found: $appManifest"
}

if (!(Test-Path -LiteralPath $assetsRoot -PathType Container)) {
    throw "MSIX assets directory was not found: $assetsRoot"
}

if ($Preflight) {
    Write-Host "MSIX packaging preflight passed:"
    Write-Host "  Repository root: $repoRoot"
    Write-Host "  Windows root: $windowsRoot"
    Write-Host "  App project: $appProject"
    Write-Host "  Package manifest: $appManifest"
    Write-Host "  DotNet path: $dotnet"
    Write-Host "  MakeAppx path: $makeAppx"
    Write-Host "  SignTool path: $signTool"
    Write-Host "  Artifact root: $artifactRoot"
    Write-Host "  Publish root: $publishRoot"
    Write-Host "  Signed package path: $packagePath"
    Write-Host "  Configuration: $Configuration"
    Write-Host "  Runtime identifier: $RuntimeIdentifier"
    Write-Host "  Timestamp server: $TimestampServerUrl"
    Write-Host "  Timestamp digest algorithm: $TimestampDigestAlgorithm"
    Write-Host ""
    Write-MsixPublishProperties
    Write-Host ""
    Write-Host "Signing options:"
    Write-Host "  PFX: -PackageCertificateKeyFile <path-to-maintainer-pfx> [-PackageCertificatePassword <secret>]"
    Write-Host "  Certificate store: -PackageCertificateThumbprint <thumbprint> [-UseLocalMachineCertificateStore]"
    Write-Host ""
    Write-Host "Signed package build command shapes:"
    Write-Host "  .\VoiceInk.Windows\scripts\package-msix.ps1 -DotNetPath `"$dotnet`" -Configuration $Configuration -RuntimeIdentifier $RuntimeIdentifier -TimestampServerUrl `"$TimestampServerUrl`" -TimestampDigestAlgorithm $TimestampDigestAlgorithm -PackageCertificateKeyFile <path-to-maintainer-pfx>"
    Write-Host "  .\VoiceInk.Windows\scripts\package-msix.ps1 -DotNetPath `"$dotnet`" -Configuration $Configuration -RuntimeIdentifier $RuntimeIdentifier -TimestampServerUrl `"$TimestampServerUrl`" -TimestampDigestAlgorithm $TimestampDigestAlgorithm -PackageCertificateThumbprint <thumbprint> -UseLocalMachineCertificateStore"
    Write-Host ""
    Write-Host "Manual smoke commands after a signed package is produced and the signing certificate is trusted on a test machine:"
    Write-Host "  .\VoiceInk.Windows\scripts\test-msix-package.ps1 -PackagePath <path-to-msix>"
    Write-Host "  Add-AppxPackage -Path <path-to-msix>"
    Write-Host "  Get-AppxPackage VoiceInk.Windows"
    Write-Host "  Remove-AppxPackage -Package <package-full-name>"
    Write-Host ""
    Write-Host "To run non-installing artifact validation immediately after a signed build, add -ValidateAfterBuild."
    Write-Host ""
    Write-Host "Preflight does not run dotnet publish, MakeAppx, SignTool, sign packages, create or import certificates, install packages, uninstall packages, or read certificate passwords."
    exit 0
}

$hasPfxSigning = ![string]::IsNullOrWhiteSpace($PackageCertificateKeyFile)
$hasThumbprintSigning = ![string]::IsNullOrWhiteSpace($PackageCertificateThumbprint)
if (!$hasPfxSigning -and !$hasThumbprintSigning) {
    throw "PackageCertificateKeyFile is required for signed packaging unless PackageCertificateThumbprint is provided. Provide a maintainer-owned signing certificate path, use a trusted store thumbprint, or run with -Preflight to validate packaging inputs without a certificate."
}

if ($hasPfxSigning -and $hasThumbprintSigning) {
    throw "Provide either PackageCertificateKeyFile or PackageCertificateThumbprint, not both."
}

if ($hasPfxSigning) {
    $PackageCertificateKeyFile = Resolve-OptionalPath -RequestedPath $PackageCertificateKeyFile -RepositoryRoot $repoRoot
}

New-Item -ItemType Directory -Force -Path $artifactRoot | Out-Null
if (Test-Path -LiteralPath $publishRoot) {
    Remove-Item -LiteralPath $publishRoot -Recurse -Force
}
if (Test-Path -LiteralPath $packagePath) {
    Remove-Item -LiteralPath $packagePath -Force
}

$binPath = Join-Path $appProjectDirectory "bin"
$objPath = Join-Path $appProjectDirectory "obj"
foreach ($buildOutputPath in @($binPath, $objPath)) {
    if (Test-Path -LiteralPath $buildOutputPath) {
        Remove-Item -LiteralPath $buildOutputPath -Recurse -Force
    }
}

Write-Host "Publishing VoiceInk for Windows package payload ($Configuration, $RuntimeIdentifier)..."
Write-MsixPublishProperties
Invoke-AndCheck -FailureMessage "dotnet publish failed." -Command {
    & $dotnet publish $appProject `
        -c $Configuration `
        -r $RuntimeIdentifier `
        --self-contained false `
        -p:Platform=x64 `
        -p:WindowsPackageType=None `
        -p:WindowsAppSDKSelfContained=false `
        -p:WindowsAppSdkBootstrapInitialize=false `
        -p:WindowsAppSdkDeploymentManagerInitialize=false `
        -p:PublishSingleFile=false `
        -o $publishRoot
}

Copy-RequiredFile -SourcePath $appManifest -DestinationPath (Join-Path $publishRoot "AppxManifest.xml") -Description "MSIX manifest"
Copy-Item -LiteralPath $assetsRoot -Destination (Join-Path $publishRoot "Assets") -Recurse -Force
Copy-XamlBinaryFiles -AppProjectDirectory $appProjectDirectory -PublishRoot $publishRoot

$appPriPath = Find-NewestFile -SearchRoot $appProjectDirectory -Filter "resources.pri" -Description "app resources.pri"
Copy-Item -LiteralPath $appPriPath -Destination (Join-Path $publishRoot "resources.pri") -Force
Copy-Item -LiteralPath $appPriPath -Destination (Join-Path $publishRoot "VoiceInk.Windows.App.pri") -Force

$runtimeRoot = Find-WindowsAppRuntimeRoot -RequestedRoot $WindowsAppRuntimePackageRoot -RepositoryRoot $repoRoot
Copy-WindowsAppRuntimePriFiles -RuntimeRoot $runtimeRoot -PublishRoot $publishRoot

Write-Host "Packing MSIX with MakeAppx..."
Invoke-AndCheck -FailureMessage "MakeAppx pack failed." -Command {
    & $makeAppx pack /d $publishRoot /p $packagePath /overwrite
}

Write-Host "Signing MSIX with SignTool..."
if ($hasPfxSigning) {
    if ([string]::IsNullOrEmpty($PackageCertificatePassword)) {
        Invoke-AndCheck -FailureMessage "SignTool signing failed." -Command {
            & $signTool sign /fd SHA256 /tr $TimestampServerUrl /td $TimestampDigestAlgorithm /f $PackageCertificateKeyFile $packagePath
        }
    }
    else {
        Invoke-AndCheck -FailureMessage "SignTool signing failed." -Command {
            & $signTool sign /fd SHA256 /tr $TimestampServerUrl /td $TimestampDigestAlgorithm /f $PackageCertificateKeyFile /p $PackageCertificatePassword $packagePath
        }
    }
}
else {
    if ($UseLocalMachineCertificateStore) {
        Invoke-AndCheck -FailureMessage "SignTool signing failed." -Command {
            & $signTool sign /fd SHA256 /tr $TimestampServerUrl /td $TimestampDigestAlgorithm /sm /sha1 $PackageCertificateThumbprint $packagePath
        }
    }
    else {
        Invoke-AndCheck -FailureMessage "SignTool signing failed." -Command {
            & $signTool sign /fd SHA256 /tr $TimestampServerUrl /td $TimestampDigestAlgorithm /sha1 $PackageCertificateThumbprint $packagePath
        }
    }
}

if ($ValidateAfterBuild) {
    $msixValidator = Join-Path $scriptRoot "test-msix-package.ps1"
    if (!(Test-Path -LiteralPath $msixValidator -PathType Leaf)) {
        throw "MSIX artifact validator was not found: $msixValidator"
    }

    Write-Host ""
    Write-Host "Validating signed MSIX artifact..."
    Invoke-AndCheck -FailureMessage "MSIX artifact validation failed." -Command {
        & $msixValidator -PackagePath $packagePath
    }

    Write-Host "Validated signed MSIX artifact:"
    Write-Host "  $packagePath"
}

Write-Host "MSIX artifacts written under:"
Write-Host "  $artifactRoot"
Write-Host ""
Write-Host "Manual smoke commands after trusting the signing certificate on a test machine:"
Write-Host "  Add-AppxPackage -Path <path-to-msix>"
Write-Host "  Get-AppxPackage VoiceInk.Windows"
Write-Host "  Remove-AppxPackage -Package <package-full-name>"
