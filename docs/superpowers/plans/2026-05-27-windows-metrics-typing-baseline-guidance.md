# Windows Metrics Typing Baseline Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add visible Metrics guidance for the local productivity estimate formula.

---

## Task 1: Red Presenter Test

- [x] Add a Core presenter test that expects a Typing Baseline data guidance row.
- [x] Run the focused Metrics presenter test and confirm it fails because the row is missing.

## Task 2: Presenter Implementation

- [x] Add the Typing Baseline row to `SessionMetricsDashboardPresenter`.
- [x] Keep existing metric calculations unchanged.

## Task 3: Verification And Commit

- [x] Run focused Metrics presenter tests, full solution tests/build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused Metrics test failed because the Typing Baseline data guidance row was absent.
- Focused Metrics presenter tests passed: 3 tests.
- Full solution tests passed: 926 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF working-copy warnings.
