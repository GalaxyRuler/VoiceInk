# Windows Metrics Accessible Dashboard Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Add UI Automation names to every repeated Metrics dashboard row type.

---

## Task 1: Red Metrics Accessibility Tests

- [x] Add focused presenter coverage expecting `AccessibleName` on dashboard cards, data guidance rows, action rows, and diagnostics rows.
- [x] Add focused static XAML coverage expecting Metrics dashboard, data, action, and diagnostics templates to bind `AutomationProperties.Name`.
- [x] Confirm the focused tests fail before implementation.

## Task 2: Presenter Names And XAML Bindings

- [x] Add computed accessible names to Metrics dashboard card and row records.
- [x] Bind Metrics action and diagnostics templates to `AutomationProperties.Name`.
- [x] Run focused Metrics/accessibility tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red focused tests failed because Metrics presenter records did not expose `AccessibleName`.
- Focused Metrics/accessibility tests passed after implementation: 5 tests.
- Full solution tests passed: Core 739, Infrastructure 266.
- Debug x64 solution build passed with 0 warnings and 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF normalization warnings.
