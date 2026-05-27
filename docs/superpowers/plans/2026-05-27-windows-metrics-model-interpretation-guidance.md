# Windows Metrics Model Interpretation Guidance Plan

## Steps

1. Add a failing Metrics dashboard presenter assertion for model performance interpretation guidance.
2. Run the focused dashboard presenter test and confirm the red failure.
3. Add the `Model Performance` data guidance row.
4. Run the focused dashboard presenter test again.
5. Update the project completion tracker.
6. Run the full solution test/build/whitespace verification gate.
7. Commit the completed slice.

## Verification

- Focused: `dotnet test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SessionMetricsDashboardPresenterTests.Present_BuildsMacStyleHeroAndMetricCards`
- Full gate: solution tests, Debug x64 build, and `git diff --check`.
