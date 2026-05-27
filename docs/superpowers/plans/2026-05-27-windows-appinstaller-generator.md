# Windows App Installer Manifest Generator Plan

## Goal

Add a safe `.appinstaller` generator that derives package identity from `Package.appxmanifest` and leaves publishing/installing/signing to maintainer-owned release infrastructure.

## Steps

1. Add failing packaging asset coverage for `write-appinstaller.ps1`.
2. Implement `write-appinstaller.ps1` with:
   - artifact-root output safety;
   - absolute URI validation;
   - manifest identity parsing;
   - App Installer 2017/2 schema output;
   - no install, publish, signing, or certificate-store mutation.
3. Extend `test-release-readiness.ps1` to include the generator.
4. Run the generator with a harmless placeholder HTTPS MSIX URI and inspect generated `MainPackage` identity.
5. Update the project completion tracker.
6. Verify with focused packaging tests, readiness script, full solution tests/build, and whitespace checking.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests
.\VoiceInk.Windows\scripts\write-appinstaller.ps1 -MainPackageUri "https://example.invalid/VoiceInk.Windows_0.1.0.0_x64.msix" -OutputPath "VoiceInk.Windows\artifacts\appinstaller-smoke\VoiceInk.Windows.appinstaller"
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```
