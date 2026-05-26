# Windows MSIX Install Trust Diagnostics Plan

## Status

Completed on 2026-05-27.

## Tasks

1. Add a failing packaging asset test for signature/trust diagnostics in `smoke-msix-install.ps1`.
2. Add read-only `Get-AuthenticodeSignature` output to the smoke plan.
3. Add MSIX trust-failure guidance for `0x800B0109`, `TrustedPeople`, and AppX deployment logs.
4. Run the focused packaging test.
5. Run the full solution test/build/diff gate.
6. Update the project completion tracker.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter WindowsPackagingAssetsTests.MsixInstallSmokeScript_PrintsPlanByDefaultAndRequiresExecuteForMutation`
- Full solution gate before commit:
  `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; & '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; git diff --check`
