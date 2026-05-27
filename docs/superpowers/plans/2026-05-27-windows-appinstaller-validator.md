# Windows App Installer Manifest Validator Plan

## Goal

Add a read-only `.appinstaller` validator so optional App Installer distribution can be checked before publishing or launching.

## Steps

1. Add failing packaging asset coverage for `test-appinstaller.ps1`.
2. Implement `test-appinstaller.ps1` with:
   - artifact-root input safety;
   - `.appinstaller` extension enforcement;
   - App Installer namespace/root/MainPackage checks;
   - `Package.appxmanifest` identity comparison;
   - absolute URI validation;
   - no publish, launch, install, signing, or certificate-store mutation.
3. Extend `test-release-readiness.ps1` to include the validator.
4. Run the validator against the generated smoke `.appinstaller` file.
5. Update the project completion tracker.
6. Verify with focused packaging tests, readiness script, full solution tests/build, and whitespace checking.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests
.\VoiceInk.Windows\scripts\test-appinstaller.ps1 -AppInstallerPath "VoiceInk.Windows\artifacts\appinstaller-smoke\VoiceInk.Windows.appinstaller"
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```
