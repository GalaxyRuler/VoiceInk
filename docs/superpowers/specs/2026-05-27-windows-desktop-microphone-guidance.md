# Windows Desktop Microphone Guidance

## Goal

Improve audio-input troubleshooting parity by naming the Windows desktop-app microphone permission users must check when VoiceInk cannot record.

## Source Of Truth

- Microsoft documents that Windows microphone privacy includes a specific desktop-app boundary: "Let desktop apps access your microphone".
- VoiceInk Windows is a desktop app and already opens `ms-settings:privacy-microphone` from the Permissions and Audio Input surfaces.
- The guidance must remain user-controlled and must not modify global privacy settings automatically.

## Requirements

- Audio Input device health guidance names the "Let desktop apps access your microphone" toggle when Windows blocks recording.
- The guidance remains a local troubleshooting hint and does not attempt registry changes, policy changes, or automatic permission mutation.
- Existing prioritized-device fallback behavior remains unchanged.

## Acceptance

- Focused tests fail before implementation because the privacy row uses generic desktop microphone wording.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
