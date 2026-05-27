# Windows Context Selected Text Action Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add visible Context Awareness action guidance for selected-text capture.

---

## Task 1: Red Presenter Test

- [x] Add a Core presenter test that expects a Selected Text action row.
- [x] Run the focused Context presenter test and confirm it fails because the action row is missing.

## Task 2: Presenter Implementation

- [x] Add the Selected Text action row to `EnhancementContextReadinessPresenter`.
- [x] Preserve existing privacy row behavior and OCR conditional rows.

## Task 3: Verification And Commit

- [x] Run focused Context presenter tests, full solution tests/build, and whitespace check.
- [x] Review the diff and fix Critical/Important findings.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused Context test failed because the Selected Text action row was absent.
- Focused Context Awareness presenter tests passed: 9 tests.
- Full solution tests passed: 927 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF working-copy warnings.
