# Windows Metrics Diagnostic Viewer Boundary Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Make the Metrics diagnostics row explicit about the Windows Diagnostic Data Viewer boundary.

---

## Task 1: Red Tests

- [x] Update Metrics presenter tests to expect Windows-owned diagnostic history/storage wording.
- [x] Confirm focused tests fail before implementation.

## Task 2: Presenter Copy

- [x] Update `SessionMetricsDashboardPresenter` diagnostics row copy.
- [x] Run the focused metrics tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: Microsoft documents Diagnostic Data Viewer and notes Windows controls diagnostic data history/storage when data viewing is enabled.
- Red: focused Metrics tests failed because the row only said VoiceInk exports are separate.
- Green: focused Metrics tests passed after adding Windows-owned diagnostic history/storage wording.
- Full test: solution test passed with 778 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
