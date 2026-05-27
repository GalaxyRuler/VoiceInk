# Windows Model Conversion Guidance Plan

## Task 1: Red Presenter Test

- [x] Add focused `ModelLibraryOverviewPresenterTests` coverage expecting a `Fine-Tuned Models` storage guidance row.
- [x] Run the focused presenter test and confirm it fails because the row is missing.

## Task 2: Core Presenter Implementation

- [x] Add the `Fine-Tuned Models` row to `ModelLibraryOverviewPresenter.StorageGuidanceRows`.
- [x] Keep the row accessible through the existing `AccessibleName` composition.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker.
- [x] Run focused Core tests for the model library presenter.
- [x] Run full solution tests and Debug x64 build sequentially.
- [x] Run `git diff --check`.
- [ ] Request code review for the committed slice and fix any Critical or Important findings.

## Verification Notes

- RED: focused `Present_ShowsFineTunedModelConversionGuidance` failed because the row was absent.
- GREEN: focused `ModelLibraryOverviewPresenterTests` passed with 9 tests after adding the row.
- Full solution tests passed: Core 801/801 and Infrastructure 267/267.
- Debug x64 build succeeded with 0 warnings and 0 errors.
- `git diff --check` exited 0 with line-ending normalization warnings only.
