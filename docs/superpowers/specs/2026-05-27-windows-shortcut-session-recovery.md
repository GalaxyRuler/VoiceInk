# Windows Shortcut Session Recovery Spec

Date: 2026-05-27

## Source of Truth

- Microsoft documents `WTSRegisterSessionNotification` as the way for a window to receive `WM_WTSSESSION_CHANGE` messages.
- `WM_WTSSESSION_CHANGE` includes lock, unlock, and desktop-ready state changes.
- VoiceInk supports Push to Talk and Hybrid recording shortcuts, where a missing key-up event at a Windows session boundary can leave the in-memory pressed state stale.

## Requirements

- Register the existing native hotkey window for session-change notifications for the current session.
- Treat lock, unlock, and desktop-ready notifications as shortcut-state boundaries.
- Clear only transient pressed shortcut state and pending middle-click activation timers.
- Keep user shortcut settings, registered hotkeys, and Power Mode shortcut assignments unchanged.
- If session notification registration is unavailable during startup, continue running without breaking shortcut registration.
- Unregister session notifications before releasing the native hotkey window handle.

## Non-Goals

- Do not change shortcut parsing or defaults.
- Do not reassign shortcuts automatically.
- Do not mutate Windows session, lock-screen, or global RDS configuration.

## Acceptance Criteria

- A focused Core test proves only lock, unlock, and desktop-ready `WM_WTSSESSION_CHANGE` events reset pressed state.
- The native global hotkey service registers/unregisters session notifications around the existing window handle.
- The native hotkey service clears in-memory pressed shortcut state after session boundary events.
- Full solution tests and Debug x64 build pass.
