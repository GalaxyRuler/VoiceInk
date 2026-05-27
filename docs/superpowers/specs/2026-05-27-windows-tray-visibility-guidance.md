# Windows Tray Visibility Guidance Design

## Context

VoiceInk for Windows adapts the macOS menu-bar shell to the Windows notification area. Windows can place notification-area icons in taskbar corner overflow, and users control which icons stay visible. The tray state currently exposed command availability but did not provide reusable guidance for hidden tray icon recovery.

Microsoft notification-area guidance notes that users have final control over tray icon visibility. VoiceInk should explain the overflow/pin behavior while preserving the tray menu as the main recovery path.

## Goal

Add tray visibility guidance to `TrayShellState`:

- guidance explains that hidden tray icons can be found in Windows taskbar corner overflow and pinned;
- guidance says the main window can always be opened from the tray menu;
- presenter tests cover the text.

## Non-Goals

- No attempt to force tray visibility.
- No registry, policy, taskbar setting, or global machine configuration changes.
- No notification spam.
- No shell behavior change.

## Testability

Focused tray shell presenter tests cover the visibility guidance string.
