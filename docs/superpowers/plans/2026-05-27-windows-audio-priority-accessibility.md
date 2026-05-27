# Windows Audio Priority Accessibility Plan

## Task 1: Red Core Tests

- [x] Add `AccessibleName` coverage to `AudioInputPriorityListTests`.
- [x] Cover a priority row with an endpoint ID.
- [x] Cover a priority row without an endpoint ID.
- [x] Run focused tests and confirm they fail because `AccessibleName` is missing.

## Task 2: Core Model Implementation

- [x] Add `PriorityDisplay` and `AccessibleName` to `PrioritizedAudioInputDevice`.
- [x] Keep priority labels one-based for the UI and accessibility text.
- [x] Shorten long endpoint IDs using the same shape as audio health rows.
- [x] Re-run focused priority-list tests.

## Verification Notes

- RED: focused `AudioInputPriorityListTests` failed at compile time because `PrioritizedAudioInputDevice.PriorityDisplay` and `AccessibleName` were absent.
- GREEN: focused `AudioInputPriorityListTests` passed with 7 tests.
- Full solution tests passed: Core 805/805 and Infrastructure 267/267.
- Debug x64 build succeeded with 0 warnings and 0 errors.
- `git diff --check` exited 0 with line-ending normalization warnings only.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker.
- [x] Run focused Core tests for audio input priority.
- [x] Run full solution tests and Debug x64 build sequentially.
- [x] Run `git diff --check`.
- [ ] Request code review for the committed slice and fix any Critical or Important findings.
