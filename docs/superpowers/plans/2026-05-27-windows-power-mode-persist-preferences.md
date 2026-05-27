# Windows Power Mode Persist Preferences Plan

**Goal:** Match macOS Power Mode session restoration by clearing manual selections after recording unless persistence is enabled.

## Steps

- [x] Add failing recorder Core tests for clearing and preserving `SelectedPowerModeRuleId`.
- [x] Add failing settings persistence coverage for `PersistPowerModeSelection`.
- [x] Add `AppSettings.PersistPowerModeSelection` with equality/hash support.
- [x] Make `DictationController` clear transient selected Power Mode after stop/cancel.
- [x] Add the WinUI `Persist Configured Preferences` checkbox and save/load wiring.
- [x] Add backup fixture coverage and update parity docs.
- [x] Run focused tests and Debug x64 build.
- [x] Run full solution tests and whitespace checks.
- [x] Commit the slice and request review.

## Verification

- Red focused Core tests failed because `AppSettings.PersistPowerModeSelection` was missing.
- Red focused Infrastructure settings test failed because `AppSettings.PersistPowerModeSelection` was missing.
- Green focused Core tests passed: 3 tests.
- Green focused Infrastructure setting test passed: 1 test.
- Debug x64 build succeeded with 0 warnings and 0 errors.
- Full solution tests passed: Core 797/797 and Infrastructure 267/267.
- Final Debug x64 build succeeded with 0 warnings and 0 errors.
- `git diff --check` exited 0 with line-ending warnings only.
