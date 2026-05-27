# Windows Tray Overflow Guidance Plan

## Steps

1. Add a failing tray presenter assertion for a menu-safe visibility guidance label.
2. Run the focused tray presenter test and confirm the red failure.
3. Add `VisibilityMenuText` to tray shell state and presenter output.
4. Render a disabled visibility help row in the native tray menu and update it from state.
5. Run the focused tray presenter test again.
6. Update the project completion tracker.
7. Run the full solution test/build/whitespace verification gate.
8. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter TrayShellPresenterTests.FromState_IdleWithLoadedSettings_EnablesStartRecording`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
