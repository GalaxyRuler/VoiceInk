# Windows Power Mode Master Toggle Plan

**Goal:** Add a global Power Mode enable/disable setting while preserving existing rules.

## Steps

- [x] Add failing matcher coverage proving disabled Power Mode ignores explicit, target, and default rules.
- [x] Add failing recorder presenter coverage proving the chooser is unavailable when Power Mode is disabled.
- [x] Add failing settings persistence coverage for `IsPowerModeEnabled`.
- [x] Add `AppSettings.IsPowerModeEnabled` with equality/hash support.
- [x] Make `PowerModeMatcher` return base settings when Power Mode is disabled.
- [x] Make `FloatingRecorderControlPresenter` show Auto and no choices when Power Mode is disabled.
- [x] Add Power Mode page checkbox and save/load/enabled-state wiring.
- [x] Add backup fixture coverage and focused verification.
- [x] Run Debug x64 build.
- [x] Run full solution tests and whitespace check.
- [x] Commit the slice and request review.

## Verification

- Red focused Core matcher test failed because `AppSettings.IsPowerModeEnabled` was missing.
- Red focused Infrastructure setting test failed because `AppSettings.IsPowerModeEnabled` was missing.
- Red focused recorder presenter test failed because the chooser still opened while Power Mode was disabled.
- Green focused Core tests passed: 11 tests.
- Green focused Infrastructure setting test passed: 1 test.
- Debug x64 build succeeded with 0 warnings and 0 errors.
- Full solution tests passed: Core 795/795 and Infrastructure 267/267.
- `git diff --check` exited 0 with line-ending warnings only.

## References

- `VoiceInk/Views/Settings/SettingsView.swift`
- `VoiceInk/AppDefaults.swift`
