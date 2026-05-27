# Windows Floating Recorder State Accessibility Plan

## Task 1: Red Presenter Test

- [x] Add a focused `FloatingRecorderPresenterTests` case expecting a recorder state accessible name.
- [x] Run the focused test and confirm it fails because the property is absent.

## Task 2: Core And WinUI Implementation

- [x] Add computed `AccessibleName` presentation data to `FloatingRecorderViewState`.
- [x] Apply the accessible name to the WinUI recorder chrome when state changes.

## Verification Notes

- RED: focused `FromState_Recording_ExposesRecorderAccessibleName` failed at compile time because `FloatingRecorderViewState.AccessibleName` was absent.
- GREEN: focused `FloatingRecorderPresenterTests` passed after adding the Core accessible name and WinUI application.
- FULL TEST: `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false` passed with Core 810/810 and Infrastructure 267/267.
- BUILD: `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false` succeeded with 0 warnings and 0 errors.
- DIFF CHECK: `git diff --check` exited 0 with only LF-to-CRLF normalization warnings.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker and parity spec.
- [x] Run focused recorder presenter tests.
- [x] Run full solution tests and Debug x64 build sequentially.
- [x] Run `git diff --check`.
- [ ] Commit and request code review.
