# Windows Microphone Permission Fallback Guidance Plan

## Steps

1. Add a failing permissions presenter assertion for microphone fallback guidance.
2. Run the focused permissions presenter test and confirm the red failure.
3. Add fallback guidance to `PermissionReadinessItem` and populate it for the microphone card.
4. Bind the fallback guidance in the Permissions checklist.
5. Run the focused test again.
6. Update the project completion tracker.
7. Run the full solution test/build/whitespace verification gate.
8. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter PermissionsReadinessPresenterTests.Build_ReturnsMacParityPermissionCardsWithWindowsActions`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
