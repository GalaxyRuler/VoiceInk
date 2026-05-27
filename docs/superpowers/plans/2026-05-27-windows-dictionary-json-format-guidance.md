# Windows Dictionary JSON Format Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Clarify that Dictionary import/export is Unicode-safe local JSON rather than CSV.

---

## Task 1: Red Tests

- [x] Update Dictionary presenter tests to expect JSON-not-CSV local backup guidance.
- [x] Add a focused test for the Unicode-safe File Format row.
- [x] Confirm focused tests fail before implementation.

## Task 2: Presenter Guidance

- [x] Update `DictionaryPagePresenter` backup copy.
- [x] Add the File Format guidance row.
- [x] Run focused Dictionary tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: Microsoft documents UTF-8 CSV/BOM handling in Excel; JSON avoids the spreadsheet encoding path for VoiceInk dictionary backups.
- Red: focused Dictionary tests failed because the presenter lacked JSON-not-CSV guidance.
- Green: focused Dictionary tests passed after adding the local JSON/Unicode-safe copy and row.
- Full test: solution test passed with 779 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
