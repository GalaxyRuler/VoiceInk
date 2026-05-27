# Windows Model Download Source Guidance Plan

## Goal

Expose open-source model provenance in the Windows local model library overview.

## Steps

- [x] Ground the slice in whisper.cpp GGML model source documentation.
- [x] Add failing Core presenter tests for the download-source row.
- [x] Add the `Download Source` storage guidance row.
- [x] Run focused model overview presenter tests.
- [x] Update the parity spec and project completion tracker.
- [x] Run full solution tests and Debug x64 build.
- [x] Review the diff and commit the slice.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~ModelLibraryOverviewPresenterTests'`
- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln`
- `& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
