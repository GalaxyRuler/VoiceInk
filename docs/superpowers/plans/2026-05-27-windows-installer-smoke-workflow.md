# Windows Installer Smoke Workflow Plan

## Goal

Add a manual self-hosted Windows workflow for signed MSIX/App Installer smoke that validates artifacts by default and only performs install/uninstall when explicitly requested.

## Steps

1. Add failing packaging asset coverage for the workflow.
2. Create `.github/workflows/windows-installer-smoke.yml`.
3. Keep the install step gated by `execute_install_smoke`.
4. Document the workflow and update project completion.
5. Verify with focused packaging tests, full solution tests/build, and whitespace checking.

## Verification

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~WindowsPackagingAssetsTests
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```
