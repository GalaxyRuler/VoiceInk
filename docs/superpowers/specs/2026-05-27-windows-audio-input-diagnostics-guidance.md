# Windows Audio Input Diagnostics Guidance Spec

Date: 2026-05-27

## Context

The Windows fork already supports System Default, Custom, and Prioritized microphone selection with endpoint-ID rebinding and unavailable-device fallback. The remaining parity gap for this slice is clearer Windows-native troubleshooting when prioritized microphones disappear and VoiceInk falls back to the system default.

Microsoft Core Audio documentation describes endpoint devices as user-facing microphones and notes that endpoints can become unavailable when unplugged, disabled, removed, or reconfigured. Microsoft microphone privacy guidance also calls out desktop-app microphone access as a system-level setting. VoiceInk should surface both routes without changing user data, installing drivers, or modifying machine settings.

## Requirements

- When all prioritized microphones are unavailable and VoiceInk uses System Default fallback, the device health rows must include microphone privacy guidance.
- The row must point users toward `ms-settings:privacy-microphone` in plain language.
- The guidance must be present in core presenter data so it can be tested without WinUI automation.
- The row must be informational and must not claim VoiceInk can change Windows privacy settings automatically.

## Non-Goals

- Do not add a WASAPI backend in this slice.
- Do not query global Windows privacy state directly.
- Do not launch Settings automatically.
- Do not change device selection behavior.

## Acceptance Criteria

- The prioritized-unavailable fallback device health rows include a `Microphone Privacy` informational row.
- The existing focused audio test covers the new row title, action badge, detail, availability, and selection state.
- Project completion documentation reflects the audio diagnostics slice.
- Focused tests, full solution tests, Debug x64 build, and whitespace validation pass.
