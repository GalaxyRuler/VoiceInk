# Windows OCR Language Support Guidance Plan

## Goal

Add local Windows OCR language-support guidance to the Context Awareness presenter.

## Steps

- [x] Ground the slice in Microsoft Windows OCR recognizer documentation.
- [x] Add a failing Core presenter test for OCR language-support guidance.
- [x] Add the OCR language-support privacy row when OCR context is enabled.
- [x] Run focused Enhancement context presenter tests.
- [x] Update the parity spec and project completion tracker.
- [x] Run full solution tests and Debug x64 build.
- [x] Review the diff and commit the slice.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~EnhancementContextReadinessPresenterTests'`
- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln`
- `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
