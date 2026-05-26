# Windows Power Mode Match Guidance Plan

## Status

Completed on 2026-05-27.

## Tasks

1. Add a failing Power Mode presenter assertion for page-level match guidance.
2. Add `MatchGuidance` to `PowerModePagePresentation`.
3. Render the guidance in the WinUI Power Mode page.
4. Run focused Power Mode presenter tests.
5. Run the full solution test/build/diff gate.
6. Update the project completion tracker.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter PowerModePagePresenterTests`
- Full solution gate before commit:
  `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; & '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; git diff --check`
