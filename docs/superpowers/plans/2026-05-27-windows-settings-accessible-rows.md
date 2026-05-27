# Windows Settings Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add presenter-backed accessibility names to Settings repeated row surfaces.

---

## Task 1: Red Presenter Tests

- [x] Add assertions for Settings action, preference, backup, and diagnostics row accessible names.
- [x] Run focused Settings presenter tests and confirm they fail before implementation because the properties are missing.

## Task 2: Presenter And XAML Binding

- [x] Add computed `AccessibleName` values to Settings row records.
- [x] Bind Settings action, preference, and backup row template roots to `AutomationProperties.Name`.
- [x] Preserve existing visible text and settings behavior.

## Task 3: Verification And Commit

- [x] Run focused Settings presenter tests.
- [x] Run full Core/Infrastructure tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red focused Settings presenter tests failed because accessible-name properties were missing.
- Focused Settings presenter tests passed: 4 tests.
- Full solution tests passed: 731 Core tests and 266 Infrastructure tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only LF-to-CRLF working-copy warnings.
