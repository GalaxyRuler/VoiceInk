# Windows Metrics Accuracy Boundary Plan

## Task 1: Red Presenter Test

- [x] Add focused `SessionMetricsDashboardPresenterTests` coverage expecting an `Accuracy Boundary` data guidance row.
- [x] Run the focused test and confirm it fails because the row is absent.

## Task 2: Core Presenter Implementation

- [x] Add the `Accuracy Boundary` row to `SessionMetricsDashboardPresenter.DataGuidanceRows`.
- [x] Keep the row accessible through the existing `AccessibleName` composition.

## Verification Notes

- RED: focused `Present_ShowsAccuracyBoundaryGuidance` failed because the Accuracy Boundary row was absent.
- GREEN: focused `SessionMetricsDashboardPresenterTests` passed after adding the Accuracy Boundary row.
- FULL TEST: `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false` passed with Core 808/808 and Infrastructure 267/267.
- BUILD: `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false` succeeded with 0 warnings and 0 errors.
- DIFF CHECK: `git diff --check` exited 0 with only LF-to-CRLF normalization warnings.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker.
- [x] Run focused Metrics presenter tests.
- [x] Run full solution tests and Debug x64 build sequentially.
- [x] Run `git diff --check`.
- [ ] Request code review for the committed slice and fix any Critical or Important findings.
