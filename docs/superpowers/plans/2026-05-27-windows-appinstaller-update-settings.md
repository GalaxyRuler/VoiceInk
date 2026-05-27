# Windows App Installer Update Settings Plan

## Goal

Add optional App Installer on-launch update settings to the generator and validator without changing the non-mutating release-safety boundary.

## Steps

1. Add failing packaging asset expectations for update-setting flags and validation text.
2. Update `write-appinstaller.ps1` with `-EnableOnLaunchUpdateCheck`, `-HoursBetweenUpdateChecks`, and 0..255 validation.
3. Update `test-appinstaller.ps1` to validate optional `UpdateSettings/OnLaunch`.
4. Smoke-run generator and validator with `HoursBetweenUpdateChecks = 6`.
5. Update the project completion tracker.
6. Verify with focused packaging tests, full solution tests/build, and whitespace checking.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests
.\VoiceInk.Windows\scripts\write-appinstaller.ps1 -MainPackageUri "https://example.invalid/VoiceInk.Windows_0.1.0.0_x64.msix" -OutputPath "VoiceInk.Windows\artifacts\appinstaller-smoke\VoiceInk.Windows-updates.appinstaller" -EnableOnLaunchUpdateCheck -HoursBetweenUpdateChecks 6
.\VoiceInk.Windows\scripts\test-appinstaller.ps1 -AppInstallerPath "VoiceInk.Windows\artifacts\appinstaller-smoke\VoiceInk.Windows-updates.appinstaller"
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```
