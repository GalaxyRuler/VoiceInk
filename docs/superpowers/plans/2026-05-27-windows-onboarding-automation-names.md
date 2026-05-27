# Windows Onboarding Automation Names Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add explicit UI Automation names to first-run setup controls and regions.

---

## Task 1: Red Accessibility Guard

- [x] Add a focused static test that expects onboarding dialog controls and readiness list regions to call `AutomationProperties.SetName`.
- [x] Confirm the focused guard fails before implementation.

## Task 2: Programmatic Automation Names

- [x] Import `Microsoft.UI.Xaml.Automation` in `MainWindow.xaml.cs`.
- [x] Set names for model path, model browser, recommended model, model download, microphone selector, microphone settings, microphone refresh, shortcut, setup actions, readiness summary, setup stages, tutorial, checklist, status, microphone status, and setup health.
- [x] Run the focused guard.

## Task 3: Verification And Commit

- [x] Run focused App XAML accessibility tests.
- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red focused guard failed because `MainWindow.xaml.cs` did not call `AutomationProperties.SetName` for onboarding controls.
- Focused onboarding automation-name guard passed after implementation: 1 test.
- Focused App XAML accessibility tests passed: 2 tests.
- Full solution tests passed: Core 734, Infrastructure 266.
- Debug x64 solution build passed with 0 warnings and 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF normalization warnings.
