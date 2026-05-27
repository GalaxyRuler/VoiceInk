# Windows Dictionary Quick Add Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add visible Dictionary guidance for Quick Add entry points.

---

## Task 1: Red Presenter Test

- [x] Add a Core presenter test that expects a Quick Add guidance row.
- [x] Run the focused Dictionary presenter test and confirm it fails because the row is missing.

## Task 2: Presenter Implementation

- [x] Add the Quick Add row to `DictionaryPagePresenter`.
- [x] Keep the existing row order stable around import/export and backup guidance.

## Task 3: Verification And Commit

- [x] Run focused Dictionary presenter tests, full solution tests/build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused Dictionary test failed because the Quick Add guidance row was absent.
- Focused Dictionary presenter tests passed: 4 tests.
- Full solution tests passed: 925 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF working-copy warnings.
