# Windows Onboarding Hero Promise Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development`.

**Goal:** Surface macOS-style onboarding hero promise lines in the Windows first-run presenter and dialog.

---

## Task 1: Red Presenter Test

- [x] Add an onboarding presenter test that expects hero tagline rows: future/new typing, writing assistant, Windows shortcut, and offline/private.
- [x] Run the focused onboarding test and confirm it fails because the presenter has no hero taglines yet.

## Task 2: Presenter And Dialog

- [x] Add presenter-backed hero taglines.
- [x] Render the taglines in the first-run dialog above setup status.
- [x] Preserve existing setup actions, summary rows, stages, tutorial steps, and save/skip behavior.

## Task 3: Verification And Commit

- [x] Run focused onboarding tests.
- [x] Run full solution tests/build and whitespace check.
- [x] Update the project completion bar and commit the slice.

## Verification Notes

- Red focused onboarding test failed with `CS1061` because `OnboardingChecklistPresentation` did not expose `HeroTaglines`.
- Focused onboarding presenter tests passed: 4 tests.
- Full solution tests passed: 929 tests.
- Full Debug x64 solution build passed: 0 warnings, 0 errors.
- `git diff --check` passed with only LF-to-CRLF working-copy warnings.
