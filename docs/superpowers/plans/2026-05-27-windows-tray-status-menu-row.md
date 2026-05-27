# Windows Tray Status Menu Row Plan

## Task 1: Red Presenter Test

- [x] Add focused `TrayShellPresenterTests` coverage expecting `StatusMenuText`.
- [x] Run the focused test and confirm it fails because the presentation value is absent.

## Task 2: Core And Native Tray Implementation

- [x] Add `StatusMenuText` to `TrayShellState` and populate it from the current status text.
- [x] Render the status row in the native tray context menu and keep it disabled.

## Verification Notes

- RED: focused `FromState_IdleWithLoadedSettings_EnablesStartRecording` failed at compile time because `TrayShellState.StatusMenuText` was absent.
- GREEN: focused `TrayShellPresenterTests` passed after adding the status row presentation.
- FULL TEST: `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false` passed with Core 810/810 and Infrastructure 267/267.
- BUILD: `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false` succeeded with 0 warnings and 0 errors.
- DIFF CHECK: `git diff --check` exited 0 with only LF-to-CRLF normalization warnings.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker and parity spec.
- [x] Run focused tray shell tests.
- [x] Run full solution tests and Debug x64 build sequentially.
- [x] Run `git diff --check`.
- [ ] Commit and request code review.
