[CmdletBinding()]
param(
    [string]$PackagePath,
    [string]$PackageName = "VoiceInk.Windows",
    [switch]$Execute,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\smoke-msix-install.ps1 -PackagePath path [-PackageName VoiceInk.Windows] [-Execute]"
    Write-Host ""
    Write-Host "Prints a signed MSIX install/query/uninstall smoke plan by default."
    Write-Host "Pass -Execute only on a test machine where the signing certificate is already trusted."
    Write-Host "PackagePath must be a .msix under VoiceInk.Windows\artifacts."
    Write-Host "This script does not create or import certificates, sign packages, build packages, or trust certificates."
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
                throw "Refusing to use reparse-point path for MSIX install smoke. Path: $currentPath"
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

function Get-SignerTrustStatus {
    param(
        [object]$SignerCertificate
    )

    if ($null -eq $SignerCertificate) {
        return "No signer certificate available for Trusted People lookup."
    }

    $signerThumbprint = [string]$SignerCertificate.Thumbprint
    if ([string]::IsNullOrWhiteSpace($signerThumbprint)) {
        return "Signer certificate thumbprint is unavailable."
    }

    $trustedPeoplePath = "Cert:\LocalMachine\TrustedPeople"
    try {
        $matchingCertificate = Get-ChildItem -LiteralPath $trustedPeoplePath -ErrorAction Stop |
            Where-Object { [string]$_.Thumbprint -eq $signerThumbprint } |
            Select-Object -First 1
        if ($null -ne $matchingCertificate) {
            return "Found signer thumbprint in $trustedPeoplePath."
        }

        return "Signer thumbprint not found in $trustedPeoplePath."
    }
    catch {
        return "Unable to read $trustedPeoplePath: $($_.Exception.Message)"
    }
}

function Write-SmokePlan {
    param(
        [string]$ResolvedPackagePath,
        [string]$ResolvedPackageName
    )

    $signature = Get-AuthenticodeSignature -FilePath $ResolvedPackagePath

    Write-Host "MSIX install smoke plan:"
    Write-Host "  Package: $ResolvedPackagePath"
    Write-Host "  Package name: $ResolvedPackageName"
    Write-Host "  Signature status: $($signature.Status)"
    if ($null -ne $signature.SignerCertificate) {
        Write-Host "  Signer certificate subject: $($signature.SignerCertificate.Subject)"
        Write-Host "  Signer certificate thumbprint: $($signature.SignerCertificate.Thumbprint)"
    }

    Write-Host "  Trust store status: $(Get-SignerTrustStatus -SignerCertificate $signature.SignerCertificate)"
    Write-Host "  Read-only trust check: Cert:\LocalMachine\TrustedPeople is inspected only when available."
    Write-Host ""
    Write-Host "Prerequisite: the package must already be signed and the signing certificate must already be trusted on this test machine."
    Write-Host "This script does not create or import certificates."
    Write-Host "Trust troubleshooting: Add-AppxPackage error 0x800B0109 usually means the package signer is not trusted on the test machine."
    Write-Host "Trust location reference: import or deploy the signer certificate to TrustedPeople outside this script before running -Execute."
    Write-Host "Deployment log reference: inspect Microsoft-Windows-AppxDeployment-Server operational logs after failed install attempts."
    Write-Host "ActivityID reference: when Add-AppxPackage or Remove-AppxPackage returns an ActivityID, run Get-AppxLog -ActivityID <activity-id>."
    Write-Host "App Installer log reference: inspect Microsoft-Windows-AppInstaller/Operational if a .appinstaller launch fails before package deployment starts."
    Write-Host ""
    Write-Host "Commands:"
    Write-Host "  Add-AppxPackage -Path `"$ResolvedPackagePath`""
    Write-Host "  `$packages = @(Get-AppxPackage -Name `"$ResolvedPackageName`")"
    Write-Host "  Get-AppxPackage -Name `"$ResolvedPackageName`""
    Write-Host "  Get-AppxPackageManifest -Package `$packages[0].PackageFullName"
    Write-Host "  Verify application id VoiceInk.Windows.App"
    Write-Host "  Remove-AppxPackage -Package `$packages[0].PackageFullName"
    Write-Host "  `$remainingPackages = @(Get-AppxPackage -Name `"$ResolvedPackageName`")"
    Write-Host "  Verify no package remains after Remove-AppxPackage"
    Write-Host ""
    Write-Host "Launch identity reference after install:"
    Write-Host "  explorer.exe shell:AppsFolder\`$PackageFamilyName!VoiceInk.Windows.App"
    Write-Host ""
    Write-Host "Pass -Execute to run this smoke on a disposable test install."
}

function Assert-SignatureReadyForExecute {
    param(
        [string]$ResolvedPackagePath
    )

    $packageSignature = Get-AuthenticodeSignature -FilePath $ResolvedPackagePath
    if ($packageSignature.Status -ne [System.Management.Automation.SignatureStatus]::Valid) {
        throw "Refusing to execute install smoke because Authenticode signature status is $($packageSignature.Status). Sign the MSIX and trust the signing certificate on this test machine before passing -Execute."
    }
}

if ($Help) {
    Show-Usage
    exit 0
}

if ([string]::IsNullOrWhiteSpace($PackagePath)) {
    throw "PackagePath is required unless -Help is used."
}

if ([string]::IsNullOrWhiteSpace($PackageName)) {
    throw "PackageName cannot be blank."
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"

if (![System.IO.Path]::IsPathRooted($PackagePath)) {
    $PackagePath = Join-Path $repoRoot $PackagePath
}

$resolvedPackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
Assert-PathInside `
    -CandidatePath $resolvedPackagePath `
    -RootPath $artifactsRoot `
    -Message "Refusing to smoke package outside artifact root."
Assert-NoReparsePointInPath -CandidatePath $resolvedPackagePath -RootPath $artifactsRoot

if (![string]::Equals([System.IO.Path]::GetExtension($resolvedPackagePath), ".msix", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "PackagePath file must be a .msix file: $resolvedPackagePath"
}

Write-SmokePlan -ResolvedPackagePath $resolvedPackagePath -ResolvedPackageName $PackageName

if (!$Execute) {
    exit 0
}

Write-Host ""
Write-Host "Executing signed MSIX install smoke..."
Assert-SignatureReadyForExecute -ResolvedPackagePath $resolvedPackagePath
Add-AppxPackage -Path $resolvedPackagePath
$installedPackages = @(Get-AppxPackage -Name $PackageName)
if ($installedPackages.Count -eq 0) {
    throw "Installed package was not found by Get-AppxPackage -Name $PackageName."
}

if ($installedPackages.Count -gt 1) {
    throw "Expected one installed package named $PackageName, found $($installedPackages.Count). Refusing to choose a package to remove."
}

$installedPackage = $installedPackages[0]
Write-Host "Installed package:"
Write-Host "  $($installedPackage.PackageFullName)"
$installedManifest = Get-AppxPackageManifest -Package $installedPackage.PackageFullName
$installedApplications = @($installedManifest.Package.Applications.Application)
$voiceInkApplication = $installedApplications |
    Where-Object { [string]$_.Id -eq "VoiceInk.Windows.App" } |
    Select-Object -First 1
if ($null -eq $voiceInkApplication) {
    throw "Installed package manifest does not contain application id VoiceInk.Windows.App."
}

Write-Host "Installed app identity:"
Write-Host "  PackageFamilyName: $($installedPackage.PackageFamilyName)"
Write-Host "  ApplicationId: $($voiceInkApplication.Id)"
Write-Host "  Launch reference: shell:AppsFolder\$($installedPackage.PackageFamilyName)!$($voiceInkApplication.Id)"
Remove-AppxPackage -Package $installedPackage.PackageFullName
Write-Host "Verifying uninstall cleanup..."
$remainingPackages = @(Get-AppxPackage -Name $PackageName)
if ($remainingPackages.Count -gt 0) {
    throw "Package still resolves after Remove-AppxPackage: $($remainingPackages.PackageFullName -join ', ')"
}

Write-Host "Uninstall cleanup verified."
Write-Host "Signed MSIX install smoke passed and package was removed."
