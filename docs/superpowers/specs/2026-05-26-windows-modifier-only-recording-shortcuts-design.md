# Windows Modifier-Only Recording Shortcuts Design

## Goal

Support macOS-style modifier-only recording shortcuts for the Windows primary and secondary recording actions while keeping utility shortcuts key-based and conservative.

## Grounding

- macOS source of truth: `VoiceInk/Shortcuts/Shortcut.swift`, `ShortcutRecorder.swift`, and `ShortcutMonitor.swift` support `modifierOnly` shortcuts and release transitions.
- Windows docs: `RegisterHotKey` registers a modifier mask plus a non-modifier virtual key; low-level keyboard hooks receive modifier key transitions. The Windows fork already uses `WH_KEYBOARD_LL` for recording key-up modes.

## Behavior

- Primary and secondary recording shortcuts may be `Ctrl`, `Alt`, `Shift`, or a combination such as `Ctrl+Alt`.
- Modifier-only shortcuts are recording-only. Paste, retry, cancel, history, quick-add, enhancement, and Power Mode shortcuts still require a non-modifier key.
- Modifier-only recording shortcuts use the existing recording mode behavior: Toggle, Push to Talk, and Hybrid.
- The native hotkey service does not call `RegisterHotKey` for modifier-only recording shortcuts. It detects them through the low-level hook and emits press/release transitions.
- Key-based recording shortcuts keep the existing `RegisterHotKey` reservation plus low-level hook press/release handling.

## Safety

- Windows-key shortcuts remain rejected.
- Modifier-only shortcuts are not exposed for utility actions because a single modifier can be easy to press accidentally.
- The hook path suppresses repeated key-down events while a recording shortcut is already pressed.

## Verification

- Core shortcut parser/registration tests cover modifier-only recording and utility rejection.
- Full solution tests and Debug x64 build must pass.
