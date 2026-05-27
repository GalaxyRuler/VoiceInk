# Windows Audio Device Health Accessibility

## Goal

Close an Audio Input accessibility gap by giving Device Health rows stable UI Automation names.

## Source Of Truth

- The macOS Audio Input page presents scan-friendly cards for System Default, Custom, and Prioritized microphones.
- VoiceInk docs describe System Default, Custom, and Prioritized input modes as first-class workflows.
- Windows microphone guidance must stay explicit about Settings > Privacy & security > Microphone and `Let desktop apps access your microphone`.

## Requirements

- `AudioInputDeviceHealthRow` exposes an `AccessibleName`.
- The accessible name combines row name, badge text, and detail in a stable scan order.
- Device rows, priority rows, fallback rows, and privacy/settings guidance rows use the same composition.
- Keep the behavior in Core presenter/model code so WinUI remains a binding surface.
- Do not query or mutate Windows privacy settings.
