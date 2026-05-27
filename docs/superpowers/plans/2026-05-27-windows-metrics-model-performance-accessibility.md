# Windows Metrics Model Performance Accessibility Plan

## Goal

Add UI Automation names to Metrics model performance rows.

## Steps

- [x] Ground the slice in Microsoft WinUI `AutomationProperties.Name` guidance.
- [x] Add failing Core presenter tests for model performance accessible names.
- [x] Add `AccessibleName` to `ModelPerformanceRow`.
- [x] Bind `AutomationProperties.Name` in transcription/enhancement model performance templates.
- [x] Run focused model performance presenter tests.
- [x] Update the parity spec and project completion tracker.
- [x] Run full solution tests and Debug x64 build.
- [x] Review the diff and commit the slice.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~ModelPerformancePresenterTests'`
- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln`
- `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
