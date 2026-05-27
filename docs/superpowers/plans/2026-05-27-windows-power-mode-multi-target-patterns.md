# Windows Power Mode Multi-Target Patterns Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Support multiple app/title/site alternatives per Power Mode rule through semicolon/newline-separated match fields.

---

## Task 1: Red Core Tests

- [x] Add matcher tests for semicolon-separated process alternatives and newline-separated URL alternatives.
- [x] Add presenter coverage for multi-target summary/guidance.
- [x] Run focused Power Mode tests and confirm failures before implementation.

## Task 2: Matching And Presentation

- [x] Split process/title/URL pattern fields on semicolons and newlines.
- [x] Match each field when any configured alternative matches.
- [x] Keep browser URL sanitization for each URL alternative.
- [x] Surface multi-target delimiter guidance in the Power Mode page presenter.

## Task 3: Verification And Commit

- [x] Run focused Power Mode tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused Power Mode tests failed because alternatives were treated as one literal pattern and guidance still used the old copy.
- Focused Power Mode matcher/page/validator tests passed: 23 tests.
- Full solution tests passed: 943 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only LF-to-CRLF working-copy warnings.
