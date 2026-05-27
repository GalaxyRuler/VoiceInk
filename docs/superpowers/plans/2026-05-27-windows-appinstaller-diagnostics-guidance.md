# Windows App Installer Diagnostics Guidance Plan

## Goal

Make signed MSIX/App Installer smoke failures diagnosable without mutating the machine or requiring access to maintainer signing secrets.

## Steps

1. Add failing packaging asset expectations for `Get-AppxLog -ActivityID` and `Microsoft-Windows-AppInstaller/Operational` in both the signed install smoke helper and release readiness report.
2. Update `smoke-msix-install.ps1` to print ActivityID and App Installer operational log guidance in its default plan output.
3. Update `test-release-readiness.ps1` to include the same guidance in the signing/trust checklist.
4. Update the project completion tracker.
5. Verify with the focused packaging tests, the read-only readiness script, full solution tests/build, and whitespace checking.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```
