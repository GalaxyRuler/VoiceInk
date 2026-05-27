# Windows Power Mode Override Guidance Plan

## Steps

1. Add a failing Power Mode page presenter assertion for override guidance.
2. Run the focused Power Mode presenter test and confirm the red failure.
3. Add `OverrideGuidance` to the presentation model.
4. Render the override guidance in the WinUI Power Mode page.
5. Run the focused Power Mode presenter test again.
6. Update the project completion tracker.
7. Run the full solution test/build/whitespace verification gate.
8. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter PowerModePagePresenterTests.Present_BuildsMacStylePowerModePageCopyAndCounts`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
