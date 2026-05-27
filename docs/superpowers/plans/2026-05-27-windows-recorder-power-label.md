# Windows Recorder Power Label Plan

**Goal:** Move the floating recorder Power Mode button label into Core and show the default indicator for Auto mode.

## Steps

- [x] Add failing recorder presenter assertions for selected-rule and automatic Power Mode button labels.
- [x] Add `PowerModeButtonLabel` to `FloatingRecorderControlState`.
- [x] Build the label in `FloatingRecorderControlPresenter`.
- [x] Bind `FloatingRecorderWindow` to the presenter-provided label.
- [x] Update recorder parity spec and project completion docs.
- [ ] Run focused recorder tests, full solution tests, Debug x64 build, and whitespace check.
- [ ] Commit the completed slice and request review.

## Verification

- Red focused recorder tests failed because `PowerModeButtonLabel` was missing.
- Green focused recorder tests passed: 4 tests.

## References

- VoiceInk docs: Power Mode button in Mini Recorder.
- `VoiceInk/Views/Recorder/RecorderComponents.swift` for the macOS recorder Power Mode control source of truth.
