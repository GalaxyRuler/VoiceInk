# Windows Recorder HelpText Accessibility Plan

**Goal:** Add UI Automation HelpText for the floating recorder state without changing visible recorder behavior.

## Plan

- [x] Add a failing presenter test for recorder `AccessibleHelpText`.
- [x] Add a failing app accessibility test for `AutomationProperties.SetHelpText`.
- [x] Confirm focused tests fail because help text is absent.
- [x] Add `AccessibleHelpText` to `FloatingRecorderViewState`.
- [x] Apply the help text to `RecorderChrome` in `FloatingRecorderWindow`.
- [x] Run focused recorder/accessibility tests, full solution tests, Debug x64 build, and `git diff --check`.

## Online Grounding

Microsoft WinUI accessibility guidance says UI Automation names provide the important label for assistive technology, while `AutomationProperties.HelpText` can carry additional explanation.

## Verification Notes

- RED: focused tests failed because `FloatingRecorderViewState.AccessibleHelpText` did not exist.
- GREEN: focused recorder/accessibility tests passed after adding presenter-backed help text and WinUI binding.
