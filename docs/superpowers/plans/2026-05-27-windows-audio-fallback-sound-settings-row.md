# Windows Audio Fallback Sound Settings Row Plan

## Status

Completed on 2026-05-27.

## Tasks

1. Add a failing audio presenter assertion for the Windows Sound settings fallback row.
2. Add the row to the all-prioritized-devices-unavailable fallback path.
3. Run the focused audio presenter test.
4. Run the full solution test/build/diff gate.
5. Update the project completion tracker.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioInputDeviceSelectionTests.DeviceHealthRows_ShowSystemDefaultFallbackWhenPrioritizedDevicesAreUnavailable`
- Full solution gate before commit:
  `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; & '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; git diff --check`
