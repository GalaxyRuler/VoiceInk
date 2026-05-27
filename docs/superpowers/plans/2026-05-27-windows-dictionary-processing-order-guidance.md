# Windows Dictionary Processing Order Guidance Plan

## Steps

1. Add a failing Dictionary page presenter assertion for processing-order guidance.
2. Run the focused dictionary presenter test and confirm the red failure.
3. Add the processing-order rule guidance row.
4. Run the focused dictionary presenter test again.
5. Update the project completion tracker.
6. Run the full solution test/build/whitespace verification gate.
7. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictionaryPagePresenterTests.Present_BuildsMacStyleSectionLabelsAndRows`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
