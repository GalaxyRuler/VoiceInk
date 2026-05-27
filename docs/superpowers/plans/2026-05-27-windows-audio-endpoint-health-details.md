# Windows Audio Endpoint Health Details Plan

## Goal

Show Core Audio endpoint identity in Audio Input Device Health rows without changing capture behavior.

## Steps

- [x] Ground the slice against Windows Core Audio endpoint documentation.
- [x] Add failing Core presenter tests for endpoint details in Custom and Prioritized rows.
- [x] Add failing coverage for long endpoint ID shortening.
- [x] Update `AudioInputDeviceHealthPresenter` to append endpoint details only for physical available devices.
- [x] Run focused audio input selection tests.
- [x] Update the parity spec and project completion tracker.
- [x] Run full solution tests and Debug x64 build.
- [x] Review the diff and commit the slice.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~AudioInputDeviceSelectionTests'`
- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln`
- `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
