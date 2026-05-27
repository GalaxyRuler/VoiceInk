# Windows Onboarding Microphone Activity Guidance Plan

## Task 1: Red Presenter Test

- [x] Add focused onboarding presenter coverage expecting recent desktop-app microphone activity guidance.
- [x] Run the focused test and confirm it fails because the guidance is absent.

## Task 2: Presenter Implementation

- [x] Update the manual privacy path setup action detail.
- [x] Keep existing onboarding flow, action count, and setup completion behavior unchanged.

## Verification Notes

- RED: focused `Present_ShowsDesktopAppMicrophonePrivacyBoundary` failed because the Manual Privacy Path row did not mention recent desktop-app microphone activity.
- GREEN: focused `OnboardingChecklistPresenterTests` passed after updating the manual privacy path row and exact collection assertion.
- FULL TEST: `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false` passed with Core 810/810 and Infrastructure 267/267.
- BUILD: `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false` succeeded with 0 warnings and 0 errors.
- DIFF CHECK: `git diff --check` exited 0 with only LF-to-CRLF normalization warnings.

## Task 3: Docs, Verification, Review, Commit

- [x] Update the project completion tracker and parity spec.
- [x] Run focused onboarding presenter tests.
- [x] Run full solution tests and Debug x64 build sequentially.
- [x] Run `git diff --check`.
- [ ] Commit and request code review.
