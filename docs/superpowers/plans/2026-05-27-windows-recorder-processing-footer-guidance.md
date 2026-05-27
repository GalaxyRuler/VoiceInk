# Windows Recorder Processing Footer Guidance Plan

## Steps

1. Add a failing floating recorder presenter assertion for transcribing and inserting footer hints.
2. Run the focused recorder presenter test and confirm the red failure.
3. Add `FooterHint` to floating recorder view state and populate it per state.
4. Bind the footer hint to the existing recorder footer text block.
5. Run the focused recorder presenter test again.
6. Update the project completion tracker.
7. Run the full solution test/build/whitespace verification gate.
8. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FloatingRecorderPresenterTests.FromState_ProcessingStates_ShowProcessingWithoutRecordingCommands`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
