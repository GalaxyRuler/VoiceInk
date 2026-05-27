# Windows Start Hidden To Tray Plan

**Goal:** Add a Windows tray-first startup setting as the open-source platform adaptation of macOS Hide Dock Icon.

## Steps

- [x] Add failing Core shell visibility tests for login startup, user-enabled tray-first launch, and first-run onboarding.
- [x] Add failing settings persistence coverage for `StartHiddenToTray`.
- [x] Add `AppSettings.StartHiddenToTray` with equality/hash support.
- [x] Add `ShellStartupVisibility.ShouldStartHidden`.
- [x] Add the General settings checkbox and load/save/enabled-state wiring.
- [x] Route startup hide/show through the Core helper.
- [x] Add backup fixture coverage and run focused backup tests.
- [x] Run focused shell/settings tests and Debug x64 build.
- [x] Run full solution tests and whitespace check.
- [ ] Commit the slice and request review.

## Verification

- Red focused Core tests failed because `ShellStartupVisibility` and `AppSettings.StartHiddenToTray` were missing.
- Red focused Infrastructure test failed because `AppSettings.StartHiddenToTray` was missing.
- Green focused Shell tests passed: 3 tests.
- Green focused settings persistence test passed: 1 test.
- Green focused backup tests passed: 9 tests.
- Debug x64 build succeeded with 0 warnings and 0 errors.
- Full solution tests passed: Core 793/793 and Infrastructure 267/267.
- `git diff --check` exited 0 with line-ending warnings only.

## References

- VoiceInk docs: General Settings.
- `VoiceInk/AppDefaults.swift`
- `VoiceInk/Views/Settings/SettingsView.swift`
