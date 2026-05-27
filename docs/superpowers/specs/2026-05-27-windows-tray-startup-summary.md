# Windows Tray Startup Summary

## Purpose

Make the Settings current-state summary reflect whether VoiceInk starts visibly or hides to the Windows notification area.

## Requirements

- Add a Settings preference summary row titled `Tray Startup`.
- Default settings should show `Show window` with visible-launch guidance.
- `StartHiddenToTray` should show `Start hidden` with tray-first guidance.
- Keep the `StartHiddenToTray` setting, startup behavior, and General checkbox unchanged.

## Non-Goals

- Do not add a second startup setting.
- Do not change launch-at-login behavior.
