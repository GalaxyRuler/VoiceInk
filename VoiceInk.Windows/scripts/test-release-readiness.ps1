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
Write-Host '  .\VoiceInk.Windows\scripts\smoke-msix-install.ps1 -PackagePath <path-to-msix>'

Write-Host ""
Write-Host "Maintainer-gated signed release commands:"
Write-Host '  .\VoiceInk.Windows\scripts\package-msix.ps1 -DotNetPath "..\.dotnet-sdk-10\dotnet.exe" -PackageCertificateKeyFile <maintainer-owned.pfx> -ValidateAfterBuild'
Write-Host '  .\VoiceInk.Windows\scripts\package-msix.ps1 -ValidateAfterBuild -PackageCertificateKeyFile <maintainer-owned.pfx>'
Write-Host '  .\VoiceInk.Windows\scripts\smoke-msix-install.ps1 -PackagePath <path-to-msix> -Execute'
Write-Host ""
Write-Host "Release signing checklist:"
Write-Host "  [ ] Publisher/certificate subject match: Package.appxmanifest Publisher must match the signing certificate subject, currently CN=VoiceInkOpenSource."
Write-Host "  [ ] Timestamp signed packages with a trusted timestamp authority so the package remains verifiable after certificate expiry."
Write-Host "  [ ] Trust prerequisite: signed MSIX install smoke must run only on a prepared test machine where the signing certificate is already trusted, for example in Trusted People."
Write-Host "  [ ] Troubleshooting: inspect Microsoft-Windows-AppxDeployment-Server and AppxPackaging operational logs for deployment, signature, and manifest failures."
Write-Host ""
Write-Host "WinApp CLI local signing reference:"
Write-Host "  [ ] Optional local development certificate flow can use winapp cert generate against Package.appxmanifest on a disposable test machine."
Write-Host "  [ ] Optional local development signing can use winapp sign against a built MSIX after verifying the manifest publisher and certificate subject."
Write-Host "  [ ] Certificate trust remains an external test-machine prerequisite; this readiness report only prints the reference and never creates or imports certificates."
Write-Host ""
Write-Host "App Installer readiness reference:"
Write-Host "  [ ] Optional .appinstaller distribution must reference the signed MSIX with a MainPackage entry."
Write-Host "  [ ] MainPackage Name/Publisher/Version must match Package.appxmanifest identity and the built MSIX package identity."
Write-Host "  [ ] Use a maintainer-owned HTTPS, network share, or local file share distribution path; this report does not generate, publish, install, or update packages."
Write-Host ""
Write-Host "Run the signed install smoke only on a disposable or prepared test machine where the signing certificate is already trusted."

if ($missingCount -gt 0) {
    throw "Release readiness report found $missingCount missing required asset(s) under $repoRoot."
}

Write-Host ""
Write-Host "Release readiness report passed."
