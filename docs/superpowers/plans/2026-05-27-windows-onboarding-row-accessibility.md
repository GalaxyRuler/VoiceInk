# Windows Onboarding Row Accessibility Plan

## Task 1: Red Core Tests

- [x] Add `OnboardingChecklistPresenterTests` coverage for setup action, summary row, stage, and tutorial accessible names.
- [x] Add coverage that setup action, summary row, and tutorial display text matches the current WinUI strings.
- [x] Run focused onboarding presenter tests and confirm they fail because the row properties are missing.

## Task 2: Core Presenter Rows

- [x] Add display/accessibility properties to onboarding row records.
- [x] Keep existing `OnboardingSetupStagePresentation.DisplayText` wording unchanged and add `AccessibleName`.
- [x] Update `MainWindow.RefreshOnboardingStatus` to use presenter `DisplayText`.
- [x] Re-run focused onboarding presenter tests.

## Verification Notes

- RED: focused `Present_RowsExposeDisplayTextAndAccessibleNames` failed at compile time because onboarding row display/accessibility properties were missing.
- GREEN: focused `OnboardingChecklistPresenterTests` passed with 6 tests.
- Full solution tests passed: Core 806/806 and Infrastructure 267/267.
- Debug x64 build succeeded with 0 warnings and 0 errors.
- `git diff --check` exited 0 with no warnings.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker.
- [x] Run focused Core tests for onboarding.
- [x] Run full solution tests and Debug x64 build sequentially.
- [x] Run `git diff --check`.
- [ ] Request code review for the committed slice and fix any Critical or Important findings.
