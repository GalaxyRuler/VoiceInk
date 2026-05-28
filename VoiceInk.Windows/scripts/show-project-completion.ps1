param(
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Usage {
    Write-Host "VoiceInk Windows project completion"
    Write-Host "Usage: .\VoiceInk.Windows\scripts\show-project-completion.ps1"
    Write-Host "This script does not install, launch, sign, trust, mutate certificates, or run GUI smoke."
}

if ($Help) {
    Write-Usage
    exit 0
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path -LiteralPath (Join-Path $scriptRoot "..\..")
$completionPath = Join-Path $repoRoot "docs\superpowers\project-completion.md"

if (!(Test-Path -LiteralPath $completionPath -PathType Leaf)) {
    throw "project-completion.md was not found: $completionPath"
}

$content = Get-Content -Raw -LiteralPath $completionPath

$overall = [regex]::Match($content, "VoiceInk Windows parity\s+\[[#\-. ]+\]\s+\d+%")
if (!$overall.Success) {
    throw "Could not find the VoiceInk Windows parity bar in project-completion.md."
}

$currentSlice = [regex]::Match(
    $content,
    '(?s)## Current Slice\s+```text\s*(?<slice>.*?)\s*```')
if (!$currentSlice.Success) {
    throw "Could not find the Current Slice block in project-completion.md."
}

Write-Host "VoiceInk Windows project completion"
Write-Host $overall.Value
Write-Host ""
Write-Host "Current Slice"
Write-Host $currentSlice.Groups["slice"].Value.Trim()
Write-Host ""
Write-Host "External release gate"
Write-Host "Completed: signed MSIX install/WACK/GUI smoke evidence passed on the maintainer-prepared disposable Windows runner."
Write-Host "Latest validated evidence: GitHub Actions run 26596430888, artifact installer-smoke-evidence."
