# Windows Modifier-Only Recording Shortcuts Implementation Plan

## Task 1: Core Representation

- Extend `GlobalShortcut` with an `IsModifierOnly` flag.
- Parse modifier-only strings such as `Ctrl`, `Alt`, `Shift`, and `Ctrl+Alt`.
- Add capture helper for modifier-only key events.
- Keep display text normalized.
- Verify with focused shortcut tests.

## Task 2: Registration Rules

- Allow modifier-only shortcuts only for `ToggleRecording` registrations.
- Reject modifier-only utility shortcuts with a clear validation error.
- Verify duplicate handling still uses normalized display text.

## Task 3: Native Hook Routing

- Skip `RegisterHotKey` for modifier-only recording shortcuts.
- Match modifier-only recording shortcuts on low-level modifier key-down when the required modifier set is active.
- Release modifier-only recording shortcuts when any required modifier key is released.
- Preserve key-based recording and utility shortcut behavior.

## Task 4: WinUI Capture And Docs

- Let the primary and secondary recording shortcut fields capture modifier-only keys.
- Keep other shortcut fields on key-based capture.
- Update README, parity spec, and completion tracker.
- Run full tests/build and commit.
