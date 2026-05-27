# Windows Model Library Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add UI Automation names to repeated AI Models library rows and catalog cards.

---

## Task 1: Red Model Library Accessibility Tests

- [x] Add focused presenter coverage expecting `AccessibleName` on model library action and storage guidance rows.
- [x] Add focused catalog coverage expecting `AccessibleName` on local Whisper model catalog cards.
- [x] Add focused static XAML coverage expecting AI Models action, storage guidance, and catalog templates to bind `AutomationProperties.Name`.
- [x] Confirm the focused tests fail before implementation.

## Task 2: Presenter Names And XAML Bindings

- [x] Add computed accessible names to model library action and storage guidance row records.
- [x] Add computed accessible names to local Whisper catalog items.
- [x] Bind AI Models action and catalog templates to `AutomationProperties.Name`.
- [x] Run focused model-library/accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red focused tests failed because model library row and catalog item records did not expose `AccessibleName`.
- Focused model-library/accessibility tests passed after implementation: 5 tests.
- Full solution tests passed: Core 744, Infrastructure 266.
- Debug x64 solution build passed with 0 warnings and 0 errors.
- `git diff --check` passed with only LF-to-CRLF normalization warnings.
