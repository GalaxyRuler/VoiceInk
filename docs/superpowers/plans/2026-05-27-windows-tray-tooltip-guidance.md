# Windows Tray Tooltip Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Make the Windows tray icon hover tooltip carry concise status plus pinning guidance.

---

## Task 1: Red Presenter Test

- [x] Add focused Core shell presenter coverage expecting `TooltipText`.
- [x] Run `TrayShellPresenterTests` and confirm it fails before implementation because the field is missing.

## Task 2: Presenter And Native Bridge

- [x] Add `TooltipText` to `TrayShellState`.
- [x] Populate it from `TrayShellPresenter` using current status plus taskbar corner overflow wording.
- [x] Set native `NotifyIcon.Text` from `state.TooltipText` while preserving the existing truncation guard.

## Task 3: Verification And Commit

- [x] Run focused tray presenter tests.
- [x] Run full Core/Infrastructure tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red focused tray presenter test failed because `TrayShellState.TooltipText` did not exist.
- Focused tray presenter tests passed: 6 tests.
- Full solution tests passed: 731 Core tests and 266 Infrastructure tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only LF-to-CRLF working-copy warnings.
