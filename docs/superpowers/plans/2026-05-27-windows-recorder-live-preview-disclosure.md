# Windows Recorder Live Preview Disclosure Plan

## Status

Completed on 2026-05-27.

## Tasks

1. Add failing floating recorder presenter assertions for live-preview detail.
2. Add `LiveTranscriptDetail` to `FloatingRecorderViewState`.
3. Populate the detail only when live preview is visible.
4. Render the detail in the WinUI floating recorder live transcript panel.
5. Run focused floating recorder presenter tests.
6. Run the full solution test/build/diff gate.
7. Update the project completion tracker.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FloatingRecorderPresenterTests`
- Full solution gate before commit:
  `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; & '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; git diff --check`
