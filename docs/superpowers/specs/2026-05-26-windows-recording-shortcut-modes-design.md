# Windows Recording Shortcut Modes Design

VoiceInk for Windows should match the macOS recording shortcut modes for primary and secondary recording shortcuts: Toggle, Push to Talk, and Hybrid.

## Source Of Truth

The macOS `RecordingShortcutManager` stores independent primary and secondary shortcut modes. Toggle starts/stops hands-free recording. Push to Talk starts on key down and stops on key up. Hybrid starts on key down, keeps recording after a short tap, and stops on release after a held press.

## Windows Adaptation

- Persist `PrimaryRecordingShortcutMode` and `SecondaryRecordingShortcutMode` in JSON settings.
- Keep default behavior as Toggle for existing users.
- Expose mode selectors below the primary and secondary shortcut fields.
- Continue using `RegisterHotKey` for every configured shortcut so Windows reserves unavailable/conflicting combinations consistently.
- Ignore duplicate `WM_HOTKEY` messages for recording shortcuts because a low-level `WH_KEYBOARD_LL` hook supplies their key-down and key-up transitions.
- Do not log raw keystrokes or rewrite user keyboard input.
- Always unhook on unregister/dispose.

## Behavior

- Toggle: key down toggles recording; key up is ignored.
- Push to Talk: key down starts recording when idle; key up stops and inserts when recording.
- Hybrid: key down starts recording. A release at or after the 0.5 second threshold stops recording; a short tap leaves recording hands-free until the next recording shortcut press.

## Completion

Completed on 2026-05-26:

- Added Core mode constants, normalization, persisted AppSettings fields, and registration metadata.
- Added Settings UI mode selectors for primary and secondary recording shortcuts.
- Added low-level keyboard-hook transitions for recording shortcuts while preserving `RegisterHotKey` for utility shortcuts.
- Routed key-down/key-up events through Windows recording control logic.
