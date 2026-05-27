# Windows Settings Diagnostics Privacy Guidance Plan

## Steps

1. Add a failing settings presenter assertion for an optional diagnostic data privacy row.
2. Run the focused settings presenter test and confirm the red failure.
3. Add the diagnostics guidance row to `SettingsSectionPresenter`.
4. Run the focused settings presenter test again.
5. Update the project completion tracker.
6. Run the full solution test/build/whitespace verification gate.
7. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SettingsSectionPresenterTests.Present_ReturnsMacStyleSettingsSectionCopy`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
