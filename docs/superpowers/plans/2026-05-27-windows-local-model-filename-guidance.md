# Windows Local Model Filename Guidance Plan

## Steps

1. Add a failing model library overview presenter assertion for expected GGML filename guidance.
2. Run the focused model overview test and confirm the red failure.
3. Add the `Expected Filename` storage guidance row.
4. Run the focused model overview test again.
5. Update the project completion tracker.
6. Run the full solution test/build/whitespace verification gate.
7. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter ModelLibraryOverviewPresenterTests.Present_EmptyLibrary_ExplainsLocalModelStartingPoint`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
