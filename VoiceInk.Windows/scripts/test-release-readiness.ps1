param(
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($Help) {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\test-release-readiness.ps1"
    Write-Host ""
    Write-Host "Prints a read-only release readiness report for VoiceInk for Windows packaging."
    Write-Host "This script does not create or import certificates, publish packages, install packages, uninstall packages, sign packages, trust certificates, or read certificate passwords."
    exit 0
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$windowsRoot = Split-Path -Parent $scriptRoot
$repoRoot = Split-Path -Parent $windowsRoot
$missingCount = 0

function Write-ReadinessCheck {
    param(
        [string]$Label,
        [string]$Path
    )

    if (Test-Path -LiteralPath $Path) {
        Write-Host "[Ready]   $Label"
        Write-Host "          $Path"
        return
    }

    $script:missingCount += 1
    Write-Host "[Missing] $Label"
    Write-Host "          $Path"
}

Write-Host "Release readiness report"
Write-Host "This script does not create or import certificates, publish packages, install packages, uninstall packages, sign packages, trust certificates, or read certificate passwords."
Write-Host ""
Write-Host "Required repository assets:"

Write-ReadinessCheck "Windows solution" (Join-Path $windowsRoot "VoiceInk.Windows.sln")
Write-ReadinessCheck "Windows app project" (Join-Path $windowsRoot "src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj")
Write-ReadinessCheck "MSIX manifest" (Join-Path $windowsRoot "src\VoiceInk.Windows.App\Package.appxmanifest")
Write-ReadinessCheck "MSIX package script" (Join-Path $scriptRoot "package-msix.ps1")
Write-ReadinessCheck "MSIX artifact validator" (Join-Path $scriptRoot "test-msix-package.ps1")
Write-ReadinessCheck "MSIX install smoke helper" (Join-Path $scriptRoot "smoke-msix-install.ps1")
Write-ReadinessCheck "App Installer manifest generator" (Join-Path $scriptRoot "write-appinstaller.ps1")
Write-ReadinessCheck "App Installer manifest validator" (Join-Path $scriptRoot "test-appinstaller.ps1")
Write-ReadinessCheck "WinGet manifest generator" (Join-Path $scriptRoot "write-winget-manifest.ps1")
Write-ReadinessCheck "WinGet manifest validator" (Join-Path $scriptRoot "test-winget-manifest.ps1")
Write-ReadinessCheck "Release checksum manifest generator" (Join-Path $scriptRoot "write-release-checksums.ps1")
Write-ReadinessCheck "Dev ZIP package script" (Join-Path $scriptRoot "package-dev-zip.ps1")
Write-ReadinessCheck "Dev ZIP smoke validator" (Join-Path $scriptRoot "test-dev-zip.ps1")
Write-ReadinessCheck "Dev ZIP per-user install helper" (Join-Path $scriptRoot "install-dev-zip.ps1")
Write-ReadinessCheck "Dev ZIP per-user uninstall helper" (Join-Path $scriptRoot "uninstall-dev-zip.ps1")

Write-Host ""
Write-Host "Non-mutating validation commands from the repository root:"
Write-Host '  .\VoiceInk.Windows\scripts\package-msix.ps1 -Preflight -DotNetPath "..\.dotnet-sdk-10\dotnet.exe"'
Write-Host '  .\VoiceInk.Windows\scripts\package-dev-zip.ps1 -DotNetPath "..\.dotnet-sdk-10\dotnet.exe"'
Write-Host '  .\VoiceInk.Windows\scripts\test-dev-zip.ps1 -PackagePath <path-to-dev-zip>'
Write-Host '  .\VoiceInk.Windows\scripts\test-msix-package.ps1 -PackagePath <path-to-msix>'
Write-Host '  .\VoiceInk.Windows\scripts\write-appinstaller.ps1 -MainPackageUri <absolute-msix-uri>'
Write-Host '  .\VoiceInk.Windows\scripts\test-appinstaller.ps1 -AppInstallerPath <path-to-appinstaller>'
Write-Host '  .\VoiceInk.Windows\scripts\write-winget-manifest.ps1 -InstallerUrl <absolute-msix-uri> -InstallerSha256 <sha256>'
Write-Host '  .\VoiceInk.Windows\scripts\test-winget-manifest.ps1 -ManifestDirectory <path-to-winget-manifest-directory>'
Write-Host '  .\VoiceInk.Windows\scripts\write-release-checksums.ps1 -ArtifactPath <artifact1>,<artifact2>'
Write-Host '  .\VoiceInk.Windows\scripts\smoke-msix-install.ps1 -PackagePath <path-to-msix>'

Write-Host ""
Write-Host "Maintainer-gated signed release commands:"
Write-Host '  .\VoiceInk.Windows\scripts\package-msix.ps1 -DotNetPath "..\.dotnet-sdk-10\dotnet.exe" -PackageCertificateKeyFile <maintainer-owned.pfx> -TimestampServerUrl "https://timestamp.acs.microsoft.com" -TimestampDigestAlgorithm SHA256 -ValidateAfterBuild'
Write-Host '  .\VoiceInk.Windows\scripts\package-msix.ps1 -ValidateAfterBuild -PackageCertificateKeyFile <maintainer-owned.pfx> -TimestampServerUrl "https://timestamp.acs.microsoft.com" -TimestampDigestAlgorithm SHA256'
Write-Host '  .\VoiceInk.Windows\scripts\smoke-msix-install.ps1 -PackagePath <path-to-msix> -Execute'
Write-Host ""
Write-Host "Release signing checklist:"
Write-Host "  [ ] Publisher/certificate subject match: Package.appxmanifest Publisher must match the signing certificate subject, currently CN=VoiceInkOpenSource."
Write-Host "  [ ] Timestamp signed packages with a trusted timestamp authority so the package remains verifiable after certificate expiry; package-msix.ps1 defaults TimestampServerUrl to https://timestamp.acs.microsoft.com and TimestampDigestAlgorithm to SHA256."
Write-Host "  [ ] Trust prerequisite: signed MSIX install smoke must run only on a prepared test machine where the signing certificate is already trusted, for example the Local Machine Trusted People store at Cert:\LocalMachine\TrustedPeople."
Write-Host "  [ ] Trust troubleshooting: Add-AppxPackage error 0x800B0109 usually means the package signer is not trusted on the test machine."
Write-Host "  [ ] Troubleshooting: inspect Microsoft-Windows-AppxDeployment-Server and AppxPackaging operational logs for deployment, signature, and manifest failures."
Write-Host "  [ ] ActivityID diagnostics: if Add-AppxPackage or Remove-AppxPackage returns an ActivityID, run Get-AppxLog -ActivityID <activity-id>."
Write-Host "  [ ] App Installer diagnostics: inspect Microsoft-Windows-AppInstaller/Operational when .appinstaller launch or update checks fail before package deployment starts."
Write-Host "  [ ] Installed identity smoke: after Add-AppxPackage, use Get-AppxPackageManifest -Package to verify application id VoiceInk.Windows.App."
Write-Host "  [ ] Launch identity reference: shell:AppsFolder\<PackageFamilyName>!VoiceInk.Windows.App can be used manually after install on the disposable runner."
Write-Host ""
Write-Host "WinApp CLI local signing reference:"
Write-Host "  [ ] Optional local development certificate flow can use winapp cert generate against Package.appxmanifest on a disposable test machine."
Write-Host "  [ ] Optional local development signing can use winapp sign against a built MSIX after verifying the manifest publisher and certificate subject."
Write-Host "  [ ] Certificate trust remains an external test-machine prerequisite; this readiness report only prints the reference and never creates or imports certificates."
Write-Host ""
Write-Host "App Installer readiness reference:"
Write-Host "  [ ] Optional .appinstaller distribution must reference the signed MSIX with a MainPackage entry."
Write-Host "  [ ] MainPackage Name/Publisher/Version must match Package.appxmanifest identity and the built MSIX package identity."
Write-Host "  [ ] Generate the optional .appinstaller file with write-appinstaller.ps1 after choosing the final signed MSIX distribution URI."
Write-Host "  [ ] Validate the optional .appinstaller file with test-appinstaller.ps1 before publishing it."
Write-Host "  [ ] Use a maintainer-owned HTTPS, network share, or local file share distribution path; this report does not generate, publish, install, or update packages."
Write-Host ""
Write-Host "WinGet manifest readiness reference:"
Write-Host "  [ ] Optional WinGet community distribution metadata can be generated with write-winget-manifest.ps1 after choosing the final signed MSIX distribution URI and InstallerSha256."
Write-Host "  [ ] The WinGet installer manifest uses InstallerType: msix and must reference the signed .msix or .msixbundle artifact."
Write-Host "  [ ] InstallerSha256 can be supplied from the maintainer release process or computed from a local package under VoiceInk.Windows\artifacts with Get-FileHash."
Write-Host "  [ ] Validate generated WinGet YAML locally with test-winget-manifest.ps1, then optionally run winget validate on a maintainer machine where WinGet is installed."
Write-Host "  [ ] This readiness report does not submit manifests, download installers, install packages, uninstall packages, sign packages, create certificates, import certificates, or trust certificates."
Write-Host ""
Write-Host "Release checksum readiness reference:"
Write-Host "  [ ] Generate SHA256SUMS.txt with write-release-checksums.ps1 after final release artifacts are staged under VoiceInk.Windows\artifacts."
Write-Host "  [ ] The checksum helper uses Get-FileHash with SHA256 and artifact-root path containment."
Write-Host "  [ ] Publish SHA256SUMS.txt next to release artifacts so users can verify downloaded files independently."
Write-Host "  [ ] This readiness report does not hash files outside VoiceInk.Windows\artifacts, publish artifacts, install packages, sign packages, create certificates, import certificates, or trust certificates."
Write-Host ""
Write-Host "SBOM readiness reference:"
Write-Host "  [ ] Generate an SPDX 2.2 SBOM after final signed MSIX, optional .appinstaller, optional WinGet YAML, and SHA256SUMS.txt artifacts are staged."
Write-Host "  [ ] Microsoft SBOM Tool (sbom-tool) is an open-source option for generating SPDX 2.2-compatible SBOM artifacts from a staged release folder."
Write-Host "  [ ] Review the generated SBOM for expected package identity, artifact paths, dependency inventory, and open-source license metadata before release publication."
Write-Host "  [ ] publish the generated SBOM next to release artifacts so downstream users can audit the open-source Windows fork release."
Write-Host "  [ ] This readiness report does not run sbom-tool, download tools, publish artifacts, install packages, sign packages, create certificates, import certificates, or trust certificates."
Write-Host ""
Write-Host "Run the signed install smoke only on a disposable or prepared test machine where the signing certificate is already trusted."

if ($missingCount -gt 0) {
    throw "Release readiness report found $missingCount missing required asset(s) under $repoRoot."
}

Write-Host ""
Write-Host "Release readiness report passed."
