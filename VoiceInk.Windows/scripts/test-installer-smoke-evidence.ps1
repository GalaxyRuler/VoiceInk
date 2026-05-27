param(
    [string]$EvidenceRoot = "",
    [switch]$RequireInstallSmoke,
    [switch]$RequireWackReport,
    [switch]$RequireGuiSmoke,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($Help) {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\test-installer-smoke-evidence.ps1 -EvidenceRoot <path> [-RequireInstallSmoke] [-RequireWackReport] [-RequireGuiSmoke]"
    Write-Host ""
    Write-Host "Validates uploaded installer-smoke evidence without installing, uninstalling, signing, trusting certificates, launching the app, or running WACK."
    exit 0
}

if ([string]::IsNullOrWhiteSpace($EvidenceRoot)) {
    throw "EvidenceRoot is required unless -Help is used."
}

function Resolve-WackReportPath {
    param(
        [string]$EvidenceRootPath,
        [string]$InstallerSummaryText
    )

    $summaryWackReportPath = ""
    foreach ($summaryLine in ($InstallerSummaryText -split "\r?\n")) {
        if ($summaryLine.StartsWith("WACK report path:", [StringComparison]::OrdinalIgnoreCase)) {
            $summaryWackReportPath = $summaryLine.Substring("WACK report path:".Length).Trim()
            break
        }
    }

    $reportFileName = "wack-report.xml"
    if (![string]::IsNullOrWhiteSpace($summaryWackReportPath)) {
        $summaryReportFileName = Split-Path -Leaf $summaryWackReportPath
        if (![string]::IsNullOrWhiteSpace($summaryReportFileName)) {
            $reportFileName = $summaryReportFileName
        }
    }

    Join-Path $EvidenceRootPath $reportFileName
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
    "WACK report path:",
    "GUI smoke requested:"
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

    $wackReportPath = Resolve-WackReportPath -EvidenceRootPath $resolvedEvidenceRoot -InstallerSummaryText $summaryText
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

if ($RequireGuiSmoke) {
    if ($summaryText.IndexOf("GUI smoke requested: true", [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "Installer smoke evidence does not show GUI smoke requested: true."
    }

    if ($summaryText.IndexOf("Install smoke executed: true", [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "GUI smoke evidence requires Install smoke executed: true."
    }

    $requiredGuiEvidenceFiles = @(
        "gui-smoke-log.txt",
        "gui-smoke-window.json",
        "gui-smoke-screenshot.png"
    )

    foreach ($guiEvidenceFile in $requiredGuiEvidenceFiles) {
        $guiEvidencePath = Join-Path $resolvedEvidenceRoot $guiEvidenceFile
        if (!(Test-Path -LiteralPath $guiEvidencePath -PathType Leaf)) {
            throw "Missing GUI smoke evidence file at $guiEvidencePath."
        }

        $guiEvidenceItem = Get-Item -LiteralPath $guiEvidencePath
        if ($guiEvidenceItem.Length -le 0) {
            throw "GUI smoke evidence file is empty at $guiEvidencePath."
        }
    }

    $guiWindowEvidencePath = Join-Path $resolvedEvidenceRoot "gui-smoke-window.json"
    try {
        $guiWindowEvidence = Get-Content -LiteralPath $guiWindowEvidencePath -Raw | ConvertFrom-Json
    }
    catch {
        throw "GUI smoke window evidence is not valid JSON at $guiWindowEvidencePath. $($_.Exception.Message)"
    }

    if ([string]::IsNullOrWhiteSpace([string]$guiWindowEvidence.packageFullName) -or
        [string]::IsNullOrWhiteSpace([string]$guiWindowEvidence.launchReference) -or
        [string]::IsNullOrWhiteSpace([string]$guiWindowEvidence.applicationId)) {
        throw "GUI smoke window evidence is missing packageFullName, launchReference, or applicationId."
    }
}

Write-Host "Installer smoke evidence validation passed."
Write-Host "This script does not install, uninstall, sign, trust certificates, launch VoiceInk, or run WACK."
