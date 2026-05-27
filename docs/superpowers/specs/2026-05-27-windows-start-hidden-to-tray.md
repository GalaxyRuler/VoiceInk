# Windows Start Hidden To Tray

## Purpose

Adapt the macOS "Hide Dock Icon" menu-bar-only behavior to Windows as a tray-first startup option.

## Source of Truth

- VoiceInk General Settings docs list "Hide Dock Icon" as the app-wide setting that runs VoiceInk as a menu-bar-only app.
- `VoiceInk/AppDefaults.swift` defaults `IsMenuBarOnly` to false.
- The Windows fork already keeps VoiceInk alive in the notification area and already starts hidden for login-startup launches.

## Requirements

- Add a persisted `StartHiddenToTray` setting defaulting to false.
- Expose the setting in the Windows General settings section as "Start hidden to tray".
- When enabled, normal app launch should hide the main shell to the tray after settings load.
- Do not hide the first-run onboarding flow; if onboarding is incomplete, the main setup flow must still show.
- Preserve existing login-startup hidden behavior regardless of the new setting.
- Keep close-to-tray, tray show/hide, shortcuts, launch-at-login registration, and single-instance activation behavior unchanged.

## Non-Goals

- Do not attempt to force notification-area icon visibility.
- Do not remove the taskbar entry while the main window is visible.
- Do not add commercial updater or announcement settings.
