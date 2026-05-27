# Windows Recording Feedback Summary Spec

## Goal

Make the Settings current-state summary reflect the full recording feedback state: sounds, system-audio mute, media pause, and resume delay.

## Source Of Truth

The macOS Settings page exposes Sound Feedback, Mute Audio While Recording, Pause Media While Recording, and Resume Delay together. The Windows app already persists and executes equivalent settings, but its summary row only described start/stop sounds.

## Windows Behavior

The Settings `Recording Feedback` summary must:

- Continue showing whether start/stop sounds are on or off.
- Include `mutes system audio` when `IsSystemMuteEnabled` is enabled.
- Include `pauses media and resumes after <delay>` when `IsPauseMediaEnabled` is enabled.
- Keep behavior unchanged. This is presenter-only copy for scan-friendly parity.

## Open-Source Boundary

The slice changes local UI presentation only. It adds no telemetry, paid support, commercial prompts, account flow, or OS mutation.

## Verification

- Add a focused Settings presenter test that fails when mute/media state is omitted.
- Run focused Settings presenter tests.
- Run full Windows tests/build and `git diff --check` before committing.
