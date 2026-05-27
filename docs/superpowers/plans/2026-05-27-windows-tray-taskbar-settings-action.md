# Windows Tray Taskbar Settings Action Plan

**Goal:** Make the tray notification-area visibility guidance row actionable by opening Windows Taskbar settings.

## Plan

- [x] Add a failing static integration test for tray taskbar settings action wiring.
- [x] Confirm the focused test fails because the event/action is absent.
- [x] Add `OpenTaskbarSettingsRequested` to `TrayIconService`.
- [x] Enable the tray visibility guidance menu item and raise the event on click.
- [x] Add a WinUI shell handler that opens `ms-settings:taskbar` with shell execution and local failure status.
- [x] Run focused tray action test, full solution tests, Debug x64 build, and `git diff --check`.

## Online Grounding

Microsoft notification-area guidance says apps should let users control notification-area icon display, and current Windows taskbar settings expose other system tray icons.

## Verification Notes

- RED: focused static test failed because `OpenTaskbarSettingsRequested` was absent.
- GREEN: focused static test passed after wiring the tray event and app handler.
