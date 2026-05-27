# Windows OCR Language Pack Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Clarify that screen OCR context depends on installed Windows OCR language packs.

---

## Task 1: Red Test

- [x] Update the context readiness test to expect Windows OCR language-pack guidance.
- [x] Confirm the focused test fails before implementation.

## Task 2: Presenter Copy

- [x] Update the OCR language support privacy row.
- [x] Run the focused test.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: Microsoft documents OCR recognizer languages as available from installed OCR language packs and says users can install new packs through Windows Settings.
- Red: focused Core test failed because the OCR language row still used generic local OCR wording.
- Green: focused Core test passed after adding installed language-pack guidance.
- Full test: solution test passed with 776 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
