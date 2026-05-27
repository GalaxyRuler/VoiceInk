# Windows Power Mode Session Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add visible Power Mode session-scope guidance to the Windows page.

---

## Task 1: Red Presenter Test

- [x] Add a Core presenter test that expects a session guidance property and copy.
- [x] Run the focused Power Mode presenter test and confirm it fails because the guidance is missing.

## Task 2: Presenter And UI Implementation

- [x] Add `SessionGuidance` to `PowerModePagePresentation`.
- [x] Populate the guidance from `PowerModePagePresenter`.
- [x] Bind the guidance to the WinUI Power Mode section with wrapped text.

## Task 3: Verification And Commit

- [x] Run focused Power Mode presenter tests, full solution tests/build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused Power Mode test failed because `SessionGuidance` was absent.
- Focused Power Mode presenter tests passed: 4 tests.
- App project Debug x64 build passed after XAML wiring: 0 warnings, 0 errors.
- Full solution tests passed: 924 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF working-copy warnings.
