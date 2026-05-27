# Windows Dictionary Replacement Examples Plan

## Goal

Expose macOS-style Word Replacement examples in the Windows Dictionary presenter.

## Steps

- [x] Ground the slice in the macOS `WordReplacementInfoPopover` and WinUI guidance-list rendering.
- [x] Add a failing Core presenter test for replacement alias/example rows.
- [x] Add presenter rows for comma-separated aliases and examples.
- [x] Run focused Dictionary presenter tests.
- [x] Update the parity spec and project completion tracker.
- [x] Run full solution tests and Debug x64 build.
- [x] Review the diff and commit the slice.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~DictionaryPagePresenterTests'`
- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln`
- `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
