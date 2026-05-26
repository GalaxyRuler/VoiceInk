# Windows History Export Scope Row Plan

## Status

Completed on 2026-05-27.

## Tasks

1. Add failing History analysis presenter assertions for the export-scope row.
2. Add the row to `HistoryAnalysisPresenter`.
3. Run focused History analysis presenter tests.
4. Run the full solution test/build/diff gate.
5. Update the project completion tracker.

## Verification

- `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter HistoryAnalysisPresenterTests`
- Full solution gate before commit:
  `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; & '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }; git diff --check`
