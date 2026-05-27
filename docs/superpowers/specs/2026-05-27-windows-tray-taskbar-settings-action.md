# Windows Tray Taskbar Settings Action Design

## Context

VoiceInk for Windows already shows notification-area visibility guidance in the tray menu and tooltip. Microsoft notification-area guidance says users should be able to control whether notification-area icons are shown, and Windows exposes that under Taskbar settings.

## Design

Make the tray visibility guidance actionable:

- Keep the tray wording as `Taskbar settings: Other system tray icons`.
- Enable that row instead of using it only as disabled guidance.
- Route the click through `TrayIconService.OpenTaskbarSettingsRequested`.
- Handle the event in the WinUI shell by launching `ms-settings:taskbar` with shell execution.
- Keep failures local as normal VoiceInk status text.

## Verification

- Static app/native tests assert the tray action event, enabled row, `ms-settings:taskbar` target, shell execution, and app handler wiring.
- Full solution tests/build and whitespace checks remain required before committing the slice.
