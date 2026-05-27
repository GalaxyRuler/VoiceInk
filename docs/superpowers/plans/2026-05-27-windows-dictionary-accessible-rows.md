# Windows Dictionary Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add presenter-backed accessibility names to Dictionary repeated row surfaces.

---

## Task 1: Red Presenter Tests

- [x] Add assertions for summary, rule guidance, vocabulary, and replacement row accessible names.
- [x] Run focused Dictionary presenter tests and confirm they fail before implementation because the properties are missing.

## Task 2: Presenter And XAML Binding

- [x] Add computed `AccessibleName` values to Dictionary row records.
- [x] Bind Dictionary Summary, Rule Guidance, Vocabulary, and Replacement templates to `AutomationProperties.Name`.
- [x] Preserve existing visible text and dictionary behavior.

## Task 3: Verification And Commit

- [x] Run focused Dictionary presenter tests.
- [x] Run full Core/Infrastructure tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red focused Dictionary presenter tests failed because accessible-name properties were missing.
- Focused Dictionary presenter tests passed: 4 tests.
- Full solution tests passed: 731 Core tests and 266 Infrastructure tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only LF-to-CRLF working-copy warnings.
