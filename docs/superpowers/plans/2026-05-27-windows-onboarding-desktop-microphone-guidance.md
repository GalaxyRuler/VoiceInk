# Windows Onboarding Desktop Microphone Guidance Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Use exact Windows desktop-app microphone privacy wording in first-run onboarding.

---

## Task 1: Red Tests

- [x] Update onboarding presenter tests to expect the exact Windows desktop-app microphone toggle phrase.
- [x] Confirm focused tests fail before implementation.

## Task 2: Presenter Copy

- [x] Update onboarding checklist, summary, and setup action copy.
- [x] Run the focused onboarding tests.

## Task 3: Verification And Commit

- [x] Run full solution tests.
- [x] Run Debug x64 solution build.
- [x] Run whitespace check and commit the slice.

## Verification Notes

- Online grounding: Microsoft documents the relevant desktop-app privacy switch as "Let desktop apps access your microphone".
- Red: focused onboarding tests failed because the presenter still used generic desktop-app access wording.
- Green: focused onboarding tests passed after updating the presenter copy and one stale expected-copy assertion.
- Full test: solution test passed with 777 Core tests and 266 Infrastructure tests.
- Build: Debug x64 solution build passed with 0 warnings and 0 errors.
- Whitespace: `git diff --check` passed with only line-ending normalization warnings.
