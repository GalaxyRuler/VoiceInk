# Windows Shortcut Recorder Design

## Goal

Make shortcut setup closer to the macOS app by letting users press a key combination in shortcut fields instead of manually typing normalized text.

## Grounding

Microsoft documents keyboard input through `KeyDown` and virtual-key values, and Windows global hotkeys use virtual-key/modifier combinations. WinUI 3 controls such as `TextBox` can handle `KeyDown`; modifier state is read from the current keyboard state before formatting the shortcut.

## Requirements

- Shortcut settings fields capture a pressed key combination and fill normalized text such as `Ctrl+Alt+Space`.
- Captured shortcuts follow the same parser/validation rules as typed shortcuts.
- Modifier-only captures show a helpful status and do not corrupt the field.
- Windows-key shortcuts remain rejected as reserved by Windows.
- Escape with no modifiers clears the focused shortcut field.
- Existing manual typing remains available.

## Non-Goals

- No low-level keyboard hook.
- No global capture while the app is unfocused.
- No Windows-key registration.
- No per-Power-Mode shortcut recording in this slice.

## Verification

- Focused shortcut tests cover capture formatting and reserved-key rejection.
- Debug x64 build validates XAML `KeyDown` wiring.
