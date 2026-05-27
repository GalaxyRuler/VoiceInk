# Windows App Installer Package URI Guard Plan

## Goal

Require App Installer `MainPackage` URIs to target `.msix` or `.msixbundle` artifacts.

## Steps

- [x] Ground the slice in Microsoft App Installer package URI documentation.
- [x] Add failing packaging asset tests for generator and validator URI-extension guards.
- [x] Add safe PowerShell URI-extension helpers to `write-appinstaller.ps1` and `test-appinstaller.ps1`.
- [x] Run focused packaging asset tests.
- [x] Update README/spec/completion tracker.
- [x] Run script-level help/invalid URI checks.
- [x] Run full solution tests and Debug x64 build.
- [x] Review the diff and commit the slice.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~WindowsPackagingAssetsTests'`
- `powershell -NoProfile -ExecutionPolicy Bypass -File VoiceInk.Windows\scripts\write-appinstaller.ps1 -Help`
- `powershell -NoProfile -ExecutionPolicy Bypass -File VoiceInk.Windows\scripts\test-appinstaller.ps1 -Help`
- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln`
- `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
