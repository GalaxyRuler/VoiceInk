# Windows Tray Status Menu Row

## Goal

Improve the Windows tray adaptation of the macOS menu-bar shell by showing the current VoiceInk status directly in the tray menu.

## Source Of Truth

- The macOS menu-bar shell exposes VoiceInk as a persistent status-oriented utility.
- Microsoft notification-area guidance says notification-area icons should make status available through the icon and menu, while keeping users in control.
- Windows already updates the tray tooltip, but the current status is easier to discover when the menu contains a scan-friendly disabled status row.

## Requirements

- Add Core tray presentation text for a disabled status menu row.
- Show the row in the native tray context menu.
- Keep the row non-interactive and keep operational command enablement unchanged.
- Do not add notifications, telemetry, commercial surfaces, or background network behavior.
