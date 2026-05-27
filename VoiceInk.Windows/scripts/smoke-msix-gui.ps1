[CmdletBinding()]
param(
    [string]$PackagePath,
    [string]$PackageName = "VoiceInk.Windows",
    [string]$ApplicationId = "VoiceInk.Windows.App",
    [string]$ProcessName = "VoiceInk.Windows.App",
    [string]$EvidenceRoot = "",
    [int]$TimeoutSeconds = 30,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\smoke-msix-gui.ps1 -PackagePath path [-EvidenceRoot path]"
    Write-Host ""
    Write-Host "Runs VoiceInk GUI smoke on a disposable Windows runner: install signed MSIX, launch app, capture window evidence and screenshot, then uninstall."
    Write-Host "PackagePath must be a signed .msix under VoiceInk.Windows\artifacts."
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
                throw "Refusing to use reparse-point path for MSIX GUI smoke. Path: $currentPath"
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

function Assert-SignatureReadyForGuiSmoke {
    param(
        [string]$ResolvedPackagePath
    )

    $packageSignature = Get-AuthenticodeSignature -FilePath $ResolvedPackagePath
    if ($packageSignature.Status -ne [System.Management.Automation.SignatureStatus]::Valid) {
        throw "Refusing to execute GUI smoke because Authenticode signature status is $($packageSignature.Status). Sign the MSIX and trust the signing certificate on this disposable runner before running GUI smoke."
    }
}

function Wait-ForProcessWindow {
    param(
        [string]$ResolvedProcessName,
        [int]$ResolvedTimeoutSeconds
    )

    $deadline = [DateTimeOffset]::UtcNow.AddSeconds($ResolvedTimeoutSeconds)
    $queryProcessName = [System.IO.Path]::GetFileNameWithoutExtension($ResolvedProcessName)
    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        $candidateProcess = Get-Process -Name $queryProcessName -ErrorAction SilentlyContinue |
            Where-Object { $_.MainWindowHandle -ne [IntPtr]::Zero } |
            Select-Object -First 1
        if ($null -ne $candidateProcess) {
            return $candidateProcess
        }

        Start-Sleep -Milliseconds 500
    }

    throw "Timed out waiting $ResolvedTimeoutSeconds seconds for process '$queryProcessName' to show a main window."
}

function Save-DesktopScreenshot {
    param(
        [string]$OutputPath
    )

    Add-Type -AssemblyName System.Windows.Forms
    Add-Type -AssemblyName System.Drawing

    $screenBounds = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bitmap = [System.Drawing.Bitmap]::new($screenBounds.Width, $screenBounds.Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen($screenBounds.Location, [System.Drawing.Point]::Empty, $screenBounds.Size)
        $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $graphics.Dispose()
        $bitmap.Dispose()
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

if ([string]::IsNullOrWhiteSpace($ApplicationId)) {
    throw "ApplicationId cannot be blank."
}

if ([string]::IsNullOrWhiteSpace($ProcessName)) {
    throw "ProcessName cannot be blank."
}

if ($TimeoutSeconds -lt 1) {
    throw "TimeoutSeconds must be at least 1."
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"

if (![System.IO.Path]::IsPathRooted($PackagePath)) {
    $PackagePath = Join-Path $repoRoot $PackagePath
}

if ([string]::IsNullOrWhiteSpace($EvidenceRoot)) {
    $EvidenceRoot = Join-Path $artifactsRoot "gha-installer-smoke"
}
elseif (![System.IO.Path]::IsPathRooted($EvidenceRoot)) {
    $EvidenceRoot = Join-Path $repoRoot $EvidenceRoot
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

$resolvedEvidenceRoot = [System.IO.Path]::GetFullPath($EvidenceRoot)
Assert-PathInside `
    -CandidatePath $resolvedEvidenceRoot `
    -RootPath $artifactsRoot `
    -Message "Refusing to write GUI smoke evidence outside artifact root."
New-Item -ItemType Directory -Force -Path $resolvedEvidenceRoot | Out-Null

$logPath = Join-Path $resolvedEvidenceRoot "gui-smoke-log.txt"
$windowEvidencePath = Join-Path $resolvedEvidenceRoot "gui-smoke-window.json"
$screenshotPath = Join-Path $resolvedEvidenceRoot "gui-smoke-screenshot.png"
$transcriptStarted = $false
$installedPackageFullName = ""

Start-Transcript -LiteralPath $logPath -Force | Out-Null
$transcriptStarted = $true

try {
    Write-Host "VoiceInk GUI smoke starting."
    Write-Host "Package: $resolvedPackagePath"
    Write-Host "Evidence root: $resolvedEvidenceRoot"
    Write-Host "Timeout seconds: $TimeoutSeconds"

    Assert-SignatureReadyForGuiSmoke -ResolvedPackagePath $resolvedPackagePath

    Add-AppxPackage -Path $resolvedPackagePath
    $installedPackages = @(Get-AppxPackage -Name $PackageName)
    if ($installedPackages.Count -eq 0) {
        throw "Installed package was not found by Get-AppxPackage -Name $PackageName."
    }

    if ($installedPackages.Count -gt 1) {
        throw "Expected one installed package named $PackageName, found $($installedPackages.Count). Refusing to choose a package to launch or remove."
    }

    $installedPackage = $installedPackages[0]
    $installedPackageFullName = $installedPackage.PackageFullName
    $installedManifest = Get-AppxPackageManifest -Package $installedPackage.PackageFullName
    $installedApplications = @($installedManifest.Package.Applications.Application)
    $voiceInkApplication = $installedApplications |
        Where-Object { [string]$_.Id -eq $ApplicationId } |
        Select-Object -First 1
    if ($null -eq $voiceInkApplication) {
        throw "Installed package manifest does not contain application id $ApplicationId."
    }

    $launchReference = "shell:AppsFolder\$($installedPackage.PackageFamilyName)!$ApplicationId"
    Write-Host "Launching $launchReference"
    Start-Process -FilePath "explorer.exe" -ArgumentList $launchReference

    $launchedProcess = Wait-ForProcessWindow -ResolvedProcessName $ProcessName -ResolvedTimeoutSeconds $TimeoutSeconds
    $windowEvidence = [ordered]@{
        packageFullName = $installedPackage.PackageFullName
        packageFamilyName = $installedPackage.PackageFamilyName
        applicationId = $ApplicationId
        launchReference = $launchReference
        processName = $launchedProcess.ProcessName
        processId = $launchedProcess.Id
        mainWindowHandle = $launchedProcess.MainWindowHandle.ToInt64()
        mainWindowTitle = $launchedProcess.MainWindowTitle
        capturedAt = [DateTimeOffset]::UtcNow.ToString("O")
    }
    $windowEvidence | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath $windowEvidencePath -Encoding UTF8

    Save-DesktopScreenshot -OutputPath $screenshotPath
    Write-Host "GUI smoke evidence captured:"
    Write-Host "  Log: $logPath"
    Write-Host "  Window: $windowEvidencePath"
    Write-Host "  Screenshot: $screenshotPath"
}
finally {
    $cleanupError = $null
    if (![string]::IsNullOrWhiteSpace($installedPackageFullName)) {
        try {
            Write-Host "Removing installed package $installedPackageFullName"
            Remove-AppxPackage -Package $installedPackageFullName
            $remainingPackages = @(Get-AppxPackage -Name $PackageName)
            if ($remainingPackages.Count -gt 0) {
                throw "Package still resolves after GUI smoke Remove-AppxPackage: $($remainingPackages.PackageFullName -join ', ')"
            }

            Write-Host "GUI smoke uninstall cleanup verified."
        }
        catch {
            $cleanupError = $_.Exception
            Write-Warning "GUI smoke uninstall cleanup failed: $($cleanupError.Message)"
        }
    }

    if ($transcriptStarted) {
        Stop-Transcript | Out-Null
    }

    if ($null -ne $cleanupError) {
        throw $cleanupError
    }
}
