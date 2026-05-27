# Windows Tray Notification Area Wording

## Goal

Make the Windows tray shell guidance use Windows-native notification-area wording while still preserving VoiceInk's tray/menu-bar adaptation language.

## Source Of Truth

- The macOS app is menu-bar first; Windows adapts that surface to the notification area.
- Microsoft documentation refers to this Windows shell surface as the notification area and notes that icons can be hidden in overflow.
- Windows 11 taskbar settings label app icons under "Other system tray icons".

## Requirements

- Update the tray visibility guidance to mention the notification area and taskbar corner overflow.
- Keep the existing menu text for the Windows 11 taskbar settings path.
- Keep the copy presenter-backed and covered by Core tests.
- Do not change tray behavior or native `NotifyIcon` wiring.

## Non-Goals

- No registry or global taskbar setting changes.
- No new shell integration API.
- No installer or shortcut changes.
