# Windows Power Mode Rule Shortcuts Design

Power Mode rules should support dedicated key+modifier shortcuts so a user can jump directly to a configured mode instead of cycling through every enabled mode. Windows implements this with the existing `RegisterHotKey` pipeline because Microsoft documents global hotkeys as modifier flags plus a virtual-key code delivered through `WM_HOTKEY`.

## Requirements

- Each enabled `PowerModeRule` may store an optional shortcut string.
- Shortcut parsing, normalization, duplicate detection, and registration reuse the existing `GlobalShortcutSettings` path.
- A Power Mode rule shortcut registers with a rule id payload.
- Pressing a Power Mode rule shortcut selects that enabled rule, persists `SelectedPowerModeRuleId`, refreshes the Power Mode list, and reports the selected mode.
- Disabled rules do not register their shortcuts.
- Duplicate shortcuts report `Power Mode: <rule name> already uses <shortcut>.`
- The Power Mode editor exposes a shortcut field with the same capture behavior as Settings shortcut fields.

## Non-Goals

- Modifier-only or key-up push-to-talk shortcuts.
- A separate low-level keyboard hook.
- Per-rule shortcuts for disabled rules.
- macOS commercial/licensing behavior.
