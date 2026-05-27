# Windows Settings Diagnostics Recent Events Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Clarify the bounded scope of VoiceInk's copied diagnostics summary in Settings.

---

## Task 1: Red Presenter Test

- [x] Add a Core presenter test that expects a Recent Events diagnostics guidance row.
- [x] Run the focused Settings presenter test and confirm it fails because the row is missing.

## Task 2: Presenter Implementation

- [x] Add the bounded recent-events row to `SettingsSectionPresenter`.
- [x] Keep existing diagnostics rows and section copy unchanged apart from the new row.

## Task 3: Verification And Commit

- [x] Run focused Settings presenter tests, full solution tests/build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused test failed because the Recent Events diagnostics row was absent.
- Focused single Settings test passed after implementation.
- A parallel focused Settings run hit a shared `obj` file lock because two `dotnet test` builds targeted the same project at once; rerunning sequentially passed.
- Full solution tests passed: 922 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF working-copy warnings.
