[CmdletBinding()]
param(
    [string]$Owner = "GalaxyRuler",
    [string]$Repo = "VoiceInk",
    [string]$Workflow = "windows-installer-smoke.yml",
    [string]$Ref = "main",
    [string]$RunnerLabel = "voiceink-windows-qa",
    [string]$SignedPackagePath = "",
    [string]$MainPackageUri = "https://example.invalid/VoiceInk.Windows_0.1.0.0_x64.msix",
    [string]$AppInstallerPath = "VoiceInk.Windows\artifacts\gha-installer-smoke\VoiceInk.Windows.appinstaller",
    [string]$WackReportPath = "VoiceInk.Windows\artifacts\gha-installer-smoke\wack-report.xml",
    [switch]$RequireRunner,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\test-installer-dispatch-readiness.ps1 -SignedPackagePath <path-on-runner-to-signed-msix> [-RequireRunner]"
    Write-Host ""
    Write-Host "Checks GitHub workflow dispatch readiness without dispatching the workflow, installing packages, signing packages, trusting certificates, or launching VoiceInk."
    Write-Host "Defaults: -Owner GalaxyRuler -Repo VoiceInk -Workflow windows-installer-smoke.yml -Ref main -RunnerLabel voiceink-windows-qa"
    Write-Host "Example dispatch shape: gh workflow run windows-installer-smoke.yml --repo GalaxyRuler/VoiceInk --ref main -f runner_label=voiceink-windows-qa -f execute_install_smoke=true -f run_wack=true -f run_gui_smoke=true"
}

function Invoke-GitHubCliReadOnly {
    param(
        [string[]]$Arguments,
        [switch]$AllowFailure
    )

    $commandText = "gh " + ($Arguments -join " ")
    $commandOutput = & gh @Arguments 2>&1
    $exitCode = $LASTEXITCODE
    $outputText = ($commandOutput | ForEach-Object { [string]$_ }) -join [Environment]::NewLine

    if ($exitCode -ne 0 -and !$AllowFailure) {
        throw "$commandText failed with exit code $exitCode. $outputText"
    }

    [pscustomobject]@{
        Succeeded = $exitCode -eq 0
        ExitCode = $exitCode
        Command = $commandText
        Output = $outputText
    }
}

if ($Help) {
    Show-Usage
    exit 0
}

if ([string]::IsNullOrWhiteSpace($Owner)) {
    throw "Owner cannot be blank."
}

if ([string]::IsNullOrWhiteSpace($Repo)) {
    throw "Repo cannot be blank."
}

if ([string]::IsNullOrWhiteSpace($Workflow)) {
    throw "Workflow cannot be blank."
}

if ([string]::IsNullOrWhiteSpace($Ref)) {
    throw "Ref cannot be blank."
}

if ([string]::IsNullOrWhiteSpace($RunnerLabel)) {
    throw "RunnerLabel cannot be blank."
}

if ([string]::IsNullOrWhiteSpace($SignedPackagePath)) {
    throw "SignedPackagePath is required. Use the path to the real signed MSIX as it will exist on the disposable Windows runner or checked-out artifact workspace."
}

if (!(Get-Command gh -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI 'gh' was not found on PATH."
}

$repoSlug = "$Owner/$Repo"
Write-Host "VoiceInk installer workflow dispatch readiness"
Write-Host "Repository: $repoSlug"
Write-Host "Workflow: $Workflow"
Write-Host "Ref: $Ref"
Write-Host "Runner label: $RunnerLabel"
Write-Host "Signed package path: $SignedPackagePath"
Write-Host ""

Write-Host "[Check] gh auth status"
Invoke-GitHubCliReadOnly -Arguments @("auth", "status", "--hostname", "github.com") | Out-Null
Write-Host "  gh auth status passed."

Write-Host "[Check] gh workflow view"
Invoke-GitHubCliReadOnly -Arguments @("workflow", "view", $Workflow, "--repo", $repoSlug) | Out-Null
Write-Host "  Workflow is visible to this token."

Write-Host "[Check] workflow file on requested ref"
$workflowFileResult = Invoke-GitHubCliReadOnly `
    -Arguments @("api", "--method", "GET", "repos/$repoSlug/contents/.github/workflows/$Workflow", "-f", "ref=$Ref") `
    -AllowFailure
if ($workflowFileResult.Succeeded) {
    Write-Host "  Workflow file exists on $Ref."
}
else {
    throw "Workflow file .github/workflows/$Workflow was not readable on ref '$Ref'. $($workflowFileResult.Output)"
}

Write-Host "[Check] repository self-hosted runners"
Write-Host "  gh api repos/$repoSlug/actions/runners"
$runnerResult = Invoke-GitHubCliReadOnly -Arguments @("api", "repos/$repoSlug/actions/runners") -AllowFailure
if ($runnerResult.Succeeded) {
    $runnerPayload = $runnerResult.Output | ConvertFrom-Json
    $matchingRunners = @($runnerPayload.runners | Where-Object {
            $runner = $_
            $runnerLabels = @($runner.labels | ForEach-Object { [string]$_.name })
            $runnerLabels -contains "self-hosted" -and
            $runnerLabels -contains "windows" -and
            $runnerLabels -contains $RunnerLabel
        })

    if ($matchingRunners.Count -eq 0) {
        throw "No repository self-hosted runner advertises labels self-hosted, windows, and $RunnerLabel."
    }

    $onlineRunners = @($matchingRunners | Where-Object { [string]$_.status -eq "online" })
    if ($onlineRunners.Count -eq 0) {
        throw "Repository self-hosted runners advertise labels self-hosted, windows, and $RunnerLabel, but none are online."
    }

    foreach ($matchingRunner in $matchingRunners) {
        Write-Host "  Runner: $($matchingRunner.name) status=$($matchingRunner.status) busy=$($matchingRunner.busy)"
    }
}
else {
    $runnerMessage = "Could not list repository self-hosted runners. This usually means the token lacks repository administration visibility. $($runnerResult.Output)"
    if ($RequireRunner) {
        throw $runnerMessage
    }

    Write-Warning $runnerMessage
}

Write-Host ""
Write-Host "Dispatch command to run after runner, signing, trust, WACK, and active desktop session are prepared:"
Write-Host "gh workflow run $Workflow --repo $repoSlug --ref $Ref -f runner_label=$RunnerLabel -f signed_package_path=`"$SignedPackagePath`" -f main_package_uri=`"$MainPackageUri`" -f appinstaller_path=`"$AppInstallerPath`" -f wack_report_path=`"$WackReportPath`" -f execute_install_smoke=true -f run_wack=true -f run_gui_smoke=true"
Write-Host ""
Write-Host "This script does not dispatch, install, uninstall, sign, trust certificates, run WACK, or launch VoiceInk."
