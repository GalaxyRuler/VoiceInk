# Windows Recorder HelpText Accessibility Design

## Context

The floating recorder already exposes a UI Automation name for the recorder chrome. Microsoft WinUI accessibility guidance treats `AutomationProperties.Name` as the required concise label and `AutomationProperties.HelpText` as the place for additional explanation when users need more state detail.

## Design

Add presenter-backed help text for the recorder chrome:

- Keep the existing accessible name concise and scan-friendly.
- Add `FloatingRecorderViewState.AccessibleHelpText` for the longer state explanation.
- Include title, status, elapsed time, footer controls, and live-preview disclosure when present.
- Apply the help text to `RecorderChrome` in `FloatingRecorderWindow.Apply`.

## Verification

- Core presenter tests cover the generated help text.
- Static WinUI accessibility tests verify `AutomationProperties.SetHelpText` is applied.
- Full solution tests/build and whitespace checks remain required before committing the slice.
