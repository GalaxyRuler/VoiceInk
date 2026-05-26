# Windows OCR Capture Scope Plan

## Status

Completed on 2026-05-27.

## Tasks

1. Add failing Core presenter assertions for full-screen and selected-region OCR privacy rows.
2. Add the OCR capture-scope row to `EnhancementContextReadinessPresenter` only when OCR context is enabled.
3. Cover the invalid constrained-region privacy row alongside the existing missing-region readiness test.
4. Run the focused presenter tests.
5. Run the full solution test/build/diff gate.
6. Update the project completion tracker.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter EnhancementContextReadinessPresenterTests`
- Full solution gate before commit:
  `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; & '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; git diff --check`
