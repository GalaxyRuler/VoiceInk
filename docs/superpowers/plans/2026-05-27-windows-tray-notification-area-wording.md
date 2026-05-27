# Windows Tray Notification Area Wording Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align tray visibility guidance with Windows notification-area terminology.

---

## Task 1: Red Presenter Test

- [x] Add a Core tray presenter test that expects notification-area/overflow wording.
- [x] Run the focused tray presenter test and confirm it fails because the old guidance is still used.

## Task 2: Presenter Copy

- [x] Update `TrayShellPresenter` visibility guidance.
- [x] Preserve current menu text and tray behavior.

## Task 3: Verification And Commit

- [x] Run focused tray presenter tests, full solution tests/build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused tray presenter test failed because the old guidance used only "tray icon" wording.
- Focused tray presenter tests passed: 6 tests.
- Full solution tests passed: 929 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF working-copy warnings.
