# Windows Custom Filler Words Settings Plan

**Goal:** Persist, expose, and apply custom filler words in the Windows cleanup pipeline.

## Steps

- [x] Add failing tests for filler-word list normalization, JSON persistence, and recorder cleanup behavior.
- [x] Add a Core `FillerWordSettings` helper for parsing and editable text rendering.
- [x] Add `AppSettings.FillerWords` with equality/hash support.
- [x] Pass the effective custom filler-word list to recorder dictation, audio-file transcription, and history retry cleanup.
- [x] Add a Settings cleanup text box and load/save it through the normalization helper.
- [x] Update the parity spec and project completion tracker.
- [x] Run focused tests, full solution tests, Debug x64 build, and whitespace check.
- [ ] Commit the completed slice and request review.

## Verification

- Red focused Core tests failed because `FillerWordSettings` and `AppSettings.FillerWords` were missing.
- Red focused Infrastructure test failed because `AppSettings.FillerWords` was missing.
- Green focused Core tests passed: 3 tests.
- Green focused Infrastructure test passed: 1 test.
- Full solution tests passed: Core 787/787 and Infrastructure 267/267.
- Debug x64 build succeeded with 0 warnings and 0 errors.
- `git diff --check` exited 0 with line-ending warnings only.

## References

- `VoiceInk/Transcription/Processing/FillerWordManager.swift`
- `VoiceInk.Windows/src/VoiceInk.Windows.Core/Text/TextPostProcessor.cs`
