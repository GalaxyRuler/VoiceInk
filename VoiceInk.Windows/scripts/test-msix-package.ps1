[CmdletBinding()]
param(
    [string]$PackagePath,
    [string]$ScratchRoot,
    [switch]$Help
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Show-Usage {
    Write-Host "Usage: .\VoiceInk.Windows\scripts\test-msix-package.ps1 -PackagePath path [-ScratchRoot path]"
    Write-Host ""
    Write-Host "Validates MSIX package contents without launching, installing, signing, or trusting certificates."
    Write-Host "This script does not cryptographically verify the package signature; it only checks that AppxSignature.p7x is present."
    Write-Host "PackagePath and ScratchRoot must stay under VoiceInk.Windows\artifacts."
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
        throw "Refusing to inspect path outside artifact root. Path: $resolvedCandidate Root: $resolvedRoot"
    }
}

function Assert-PathOutside {
    param(
        [string]$CandidatePath,
        [string]$RootPath,
        [string]$Message
    )

    $resolvedRoot = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $resolvedCandidate = [System.IO.Path]::GetFullPath($CandidatePath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    if ($resolvedCandidate.StartsWith($resolvedRoot + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase) -or
        [string]::Equals($resolvedCandidate, $resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "$Message Path: $resolvedCandidate Root: $resolvedRoot"
    }
}

function Assert-FileExists {
    param(
        [string]$RootPath,
        [string]$RelativePath
    )

    $filePath = Join-Path $RootPath $RelativePath
    if (!(Test-Path -LiteralPath $filePath -PathType Leaf)) {
        throw "Expected MSIX package file missing: $RelativePath"
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
                throw "Refusing to use reparse-point path for MSIX smoke validation. Path: $currentPath"
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

function Assert-TextDoesNotContainCommercialWords {
    param(
        [string]$Text,
        [string]$FileName
    )

    $forbiddenWords = @("trial", "purchase", "store", "license", "paid")
    foreach ($word in $forbiddenWords) {
        if ($Text.IndexOf($word, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "MSIX $FileName contains forbidden commercial wording: $word"
        }
    }
}

if ($Help) {
    Show-Usage
    exit 0
}

if ([string]::IsNullOrWhiteSpace($PackagePath)) {
    throw "PackagePath is required unless -Help is used."
}

$scriptRoot = $PSScriptRoot
$windowsRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $windowsRoot "..")).Path
$artifactsRoot = Join-Path $windowsRoot "artifacts"

if (![System.IO.Path]::IsPathRooted($PackagePath)) {
    $PackagePath = Join-Path $repoRoot $PackagePath
}

$resolvedPackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
Assert-PathInside -CandidatePath $resolvedPackagePath -RootPath $artifactsRoot
Assert-NoReparsePointInPath -CandidatePath $resolvedPackagePath -RootPath $artifactsRoot

if (![string]::Equals([System.IO.Path]::GetExtension($resolvedPackagePath), ".msix", [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "PackagePath file must be a .msix file: $resolvedPackagePath"
}

if ([string]::IsNullOrWhiteSpace($ScratchRoot)) {
    $ScratchRoot = Join-Path $artifactsRoot "smoke"
}
elseif (![System.IO.Path]::IsPathRooted($ScratchRoot)) {
    $ScratchRoot = Join-Path $repoRoot $ScratchRoot
}

$scratchRootFull = [System.IO.Path]::GetFullPath($ScratchRoot)
Assert-PathInside -CandidatePath $scratchRootFull -RootPath $artifactsRoot
Assert-NoReparsePointInPath -CandidatePath $scratchRootFull -RootPath $artifactsRoot

$extractRoot = Join-Path $scratchRootFull "msix"
Assert-PathInside -CandidatePath $extractRoot -RootPath $artifactsRoot
Assert-NoReparsePointInPath -CandidatePath $extractRoot -RootPath $artifactsRoot
Assert-PathOutside -CandidatePath $resolvedPackagePath -RootPath $extractRoot -Message "Refusing to use a package inside the extraction cleanup root."

if (Test-Path -LiteralPath $extractRoot) {
    Remove-Item -LiteralPath $extractRoot -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $extractRoot | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory($resolvedPackagePath, $extractRoot)

Assert-FileExists -RootPath $extractRoot -RelativePath "AppxManifest.xml"
Assert-FileExists -RootPath $extractRoot -RelativePath "AppxBlockMap.xml"
Assert-FileExists -RootPath $extractRoot -RelativePath "AppxSignature.p7x"
Assert-FileExists -RootPath $extractRoot -RelativePath "VoiceInk.Windows.App.exe"
Assert-FileExists -RootPath $extractRoot -RelativePath "LICENSE.txt"

$licenseText = Get-Content -Raw -LiteralPath (Join-Path $extractRoot "LICENSE.txt")
if ($licenseText -notmatch "GNU GENERAL PUBLIC LICENSE") {
    throw "MSIX LICENSE.txt must include the GNU GPL license text."
}

$manifestPath = Join-Path $extractRoot "AppxManifest.xml"
$manifestText = Get-Content -Raw -LiteralPath $manifestPath
Assert-TextDoesNotContainCommercialWords -Text $manifestText -FileName "AppxManifest.xml"

[xml]$manifest = $manifestText
$appxNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10"
$restrictedNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"
$namespaceManager = New-Object System.Xml.XmlNamespaceManager($manifest.NameTable)
$namespaceManager.AddNamespace("appx", $appxNamespace)
$namespaceManager.AddNamespace("rescap", $restrictedNamespace)

$identity = $manifest.SelectSingleNode("/appx:Package/appx:Identity", $namespaceManager)
if ($null -eq $identity -or $identity.Name -ne "VoiceInk.Windows" -or $identity.Publisher -ne "CN=VoiceInkOpenSource") {
    throw "MSIX manifest identity must be VoiceInk.Windows / CN=VoiceInkOpenSource."
}

$targetDeviceFamily = $manifest.SelectSingleNode("/appx:Package/appx:Dependencies/appx:TargetDeviceFamily", $namespaceManager)
if ($null -eq $targetDeviceFamily -or
    $targetDeviceFamily.Name -ne "Windows.Desktop" -or
    $targetDeviceFamily.MinVersion -ne "10.0.19041.0" -or
    $targetDeviceFamily.MaxVersionTested -ne "10.0.26100.0") {
    throw "MSIX manifest TargetDeviceFamily must be Windows.Desktop with MinVersion 10.0.19041.0 and MaxVersionTested 10.0.26100.0."
}

$application = $manifest.SelectSingleNode("/appx:Package/appx:Applications/appx:Application", $namespaceManager)
if ($null -eq $application -or $application.Executable -ne "VoiceInk.Windows.App.exe") {
    throw "MSIX manifest application executable must be VoiceInk.Windows.App.exe."
}

$microphoneCapability = $manifest.SelectSingleNode("/appx:Package/appx:Capabilities/appx:DeviceCapability[@Name='microphone']", $namespaceManager)
if ($null -eq $microphoneCapability) {
    throw "MSIX manifest must include microphone capability."
}

$fullTrustCapability = $manifest.SelectSingleNode("/appx:Package/appx:Capabilities/rescap:Capability[@Name='runFullTrust']", $namespaceManager)
if ($null -eq $fullTrustCapability) {
    throw "MSIX manifest must include runFullTrust capability."
}

Write-Host "MSIX artifact validation passed:"
Write-Host "  $extractRoot"
Write-Host ""
Write-Host "Manual signed MSIX smoke commands after trusting the signing certificate on a test machine:"
Write-Host "  Add-AppxPackage -Path `"$resolvedPackagePath`""
Write-Host "  Get-AppxPackage VoiceInk.Windows"
Write-Host "  Remove-AppxPackage -Package <package-full-name>"
