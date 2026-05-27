# Windows Audio Device Health Accessibility Plan

## Task 1: Red Core Tests

- [x] Add `AudioInputDeviceHealthPresenterTests`.
- [x] Cover `AccessibleName` for a selected custom device.
- [x] Cover `AccessibleName` for the prioritized fallback privacy guidance row.
- [x] Run focused tests and confirm they fail because `AccessibleName` is missing.

## Task 2: Core Row Model

- [x] Add `AccessibleName` to `AudioInputDeviceHealthRow`.
- [x] Compose it from `Name`, `BadgeText`, and `Detail`, trimming empty parts.
- [x] Re-run focused audio presenter tests.

## Verification Notes

- RED: focused `AudioInputDeviceHealthPresenterTests` failed at compile time because `AudioInputDeviceHealthRow.AccessibleName` was absent.
- GREEN: focused `AudioInputDeviceHealthPresenterTests` passed with 2 tests.
- Full solution tests passed: Core 803/803 and Infrastructure 267/267.
- Debug x64 build succeeded with 0 warnings and 0 errors.
- `git diff --check` exited 0 with line-ending normalization warnings only.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker.
- [x] Run focused Core tests for audio input device health.
- [x] Run full solution tests and Debug x64 build sequentially.
- [x] Run `git diff --check`.
- [ ] Request code review for the committed slice and fix any Critical or Important findings.
