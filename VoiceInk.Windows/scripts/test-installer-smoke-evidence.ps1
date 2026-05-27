param(
    [string]$EvidenceRoot = "",
    [switch]$RequireInstallSmoke,
    [switch]$RequireWackReport,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($Help) {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\test-installer-smoke-evidence.ps1 -EvidenceRoot <path> [-RequireInstallSmoke] [-RequireWackReport]"
    Write-Host ""
    Write-Host "Validates uploaded installer-smoke evidence without installing, uninstalling, signing, trusting certificates, launching the app, or running WACK."
    exit 0
}

if ([string]::IsNullOrWhiteSpace($EvidenceRoot)) {
    throw "EvidenceRoot is required unless -Help is used."
}

$resolvedEvidenceRoot = (Resolve-Path -LiteralPath $EvidenceRoot).Path
$summaryPath = Join-Path $resolvedEvidenceRoot "installer-smoke-summary.txt"
if (!(Test-Path -LiteralPath $summaryPath -PathType Leaf)) {
    throw "Missing installer smoke summary at $summaryPath."
}

$summaryText = Get-Content -LiteralPath $summaryPath -Raw
$requiredSummaryFragments = @(
    "Signed package path:",
    "Install smoke executed:",
    "Windows App Certification Kit requested:",
    "WACK report path:"
)

foreach ($fragment in $requiredSummaryFragments) {
    if ($summaryText.IndexOf($fragment, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Installer smoke summary is missing required field '$fragment'."
    }
}

if ($RequireInstallSmoke -and $summaryText.IndexOf("Install smoke executed: true", [StringComparison]::OrdinalIgnoreCase) -lt 0) {
    throw "Installer smoke evidence does not show Install smoke executed: true."
}

if ($RequireWackReport) {
    if ($summaryText.IndexOf("Windows App Certification Kit requested: true", [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Installer smoke evidence does not show Windows App Certification Kit requested: true."
    }

    $wackReportPath = Join-Path $resolvedEvidenceRoot "wack-report.xml"
    if (!(Test-Path -LiteralPath $wackReportPath -PathType Leaf)) {
        throw "Missing WACK report at $wackReportPath."
    }

    $wackReportItem = Get-Item -LiteralPath $wackReportPath
    if ($wackReportItem.Length -le 0) {
        throw "WACK report is empty at $wackReportPath."
    }

    try {
        [xml]$wackReportDocument = Get-Content -LiteralPath $wackReportPath -Raw
    }
    catch {
        throw "WACK report is not valid XML at $wackReportPath. $($_.Exception.Message)"
    }

    if ($null -eq $wackReportDocument.DocumentElement) {
        throw "WACK report XML has no document element at $wackReportPath."
    }
}

Write-Host "Installer smoke evidence validation passed."
Write-Host "This script does not install, uninstall, sign, trust certificates, launch VoiceInk, or run WACK."
