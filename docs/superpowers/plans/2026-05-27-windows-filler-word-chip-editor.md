# Windows Filler Word Chip Editor Plan

**Goal:** Replace the Windows multiline filler-word field with a macOS-style add/remove chip editor.

## Steps

- [x] Add failing Core helper tests for add, duplicate rejection, and remove behavior.
- [x] Add Core helper methods for add/remove list editing.
- [x] Replace the Settings multiline field with add field, add icon button, and chip-like removable item list.
- [x] Wire Enter key add, add button, remove buttons, load state, and Apply-state persistence.
- [x] Run focused helper tests and Debug x64 build.
- [x] Run full solution tests and whitespace check.
- [ ] Commit the slice and request review.

## Verification

- Red focused helper tests failed because `TryAdd` and `Remove` were missing.
- Green focused helper tests passed: 5 tests.
- Initial Debug x64 build failed because `Windows.System.VirtualKey` was shadowed by the app namespace.
- After qualifying `global::Windows.System.VirtualKey.Enter`, Debug x64 build succeeded with 0 warnings and 0 errors.
- Full solution tests passed: Core 790/790 and Infrastructure 267/267.
- `git diff --check` exited 0 with line-ending warnings only.

## References

- VoiceInk docs: Filler Words.
- `VoiceInk/Views/Components/FillerWordsSettingsView.swift`
