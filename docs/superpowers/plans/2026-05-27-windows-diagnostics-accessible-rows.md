# Windows Diagnostics Accessible Rows Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Bind About / Open Source diagnostics guidance rows to their presenter-backed accessible names.

---

## Task 1: Red XAML Guard

- [x] Add a focused static XAML test that locates `DiagnosticsGuidanceListView` and expects its row template to bind `AutomationProperties.Name`.
- [x] Run the focused XAML guard and confirm it fails before implementation.

## Task 2: XAML Binding

- [x] Add the `AutomationProperties.Name="{Binding AccessibleName}"` binding to the diagnostics guidance template root.
- [x] Run the focused XAML guard plus Settings presenter tests.

## Task 3: Verification And Commit

- [x] Run full Core/Infrastructure tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Red XAML accessibility guard failed because `DiagnosticsGuidanceListView` did not bind `AccessibleName`.
- Focused XAML guard plus Settings presenter tests passed: 5 tests.
- Full solution tests passed: Core 732, Infrastructure 266.
- Debug x64 solution build passed with 0 warnings and 0 errors.
- `git diff --check` passed with only existing LF-to-CRLF normalization warnings.
