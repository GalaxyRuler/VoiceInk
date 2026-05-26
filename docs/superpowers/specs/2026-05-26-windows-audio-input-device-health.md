# Windows Audio Input Device Health Spec

Date: 2026-05-26

## Goal

Match the macOS Audio Input affordance more closely by showing which microphone is active, which devices are available, and which prioritized devices are currently unavailable.

## Source Of Truth

VoiceInk's Audio Input docs describe three modes:

- System Default follows the OS microphone.
- Custom Device locks VoiceInk to a chosen microphone and shows the active device with an `Active` badge.
- Prioritized tries devices in order and falls back when higher-priority devices are unavailable.

## Windows Behavior

- Add a Core presenter for device health rows so the behavior is testable without WinUI automation.
- Render a `Device Health` list on the Windows Audio Input settings page.
- In System Default or Custom Device mode, show System Default plus each physical input.
- In Prioritized mode, show the configured priority list first, including unavailable saved devices that are not in the current Windows device list.
- Use badge text:
  - `Active` for the currently selected physical microphone or System Default.
  - `Available` for detected physical microphones that are not active.
  - `Unavailable` for prioritized devices that are not detected.
  - `Default` for System Default when it is visible but not selected.
- Keep the slice local-only. Do not add telemetry, cloud checks, account flows, or commercial support surfaces.

## Non-Goals

- Do not change microphone capture selection.
- Do not add audio probing or recording tests.
- Do not introduce a new audio backend.

