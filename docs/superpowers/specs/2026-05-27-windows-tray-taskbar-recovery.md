# Windows Tray Taskbar Recovery Spec

Date: 2026-05-27

## Source of Truth

- Microsoft documents that when the Windows taskbar is created, the shell broadcasts a registered `TaskbarCreated` message and notification-area apps should assume their icons were removed and add them again.
- VoiceInk for Windows uses a Windows notification-area icon as the macOS menu-bar equivalent, so it should survive Explorer/taskbar restarts as gracefully as practical.

## Requirements

- Register for the shell `TaskbarCreated` message while the tray icon service is alive.
- Re-add the notification-area icon after that message without changing the visible tray menu, tooltip, icon, or current app state.
- Ignore unrelated window messages and invalid registration values.
- Dispose any hidden message window when the tray icon service is disposed.
- Keep the implementation local-only and open-source; no telemetry or commercial shell integration.

## Non-Goals

- Do not mutate Windows taskbar settings or pin the icon automatically.
- Do not restart Explorer or modify global shell configuration.
- Do not replace the existing `System.Windows.Forms.NotifyIcon` implementation.

## Acceptance Criteria

- A focused Core test proves the recovery gate only accepts the registered taskbar-created message.
- The native tray implementation registers a hidden message window and forces a `NotifyIcon` re-add on recovery.
- The Windows solution test/build passes after the slice.
