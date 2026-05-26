# Windows Power Mode Auto-Send Design

Power Mode rules should match the macOS app's `autoSendKey` workflow: after a successful transcription paste, a rule can send Return, Shift+Return, or Command+Return. On Windows, Command+Return is adapted to Ctrl+Return while keeping the stored raw value `commandEnter` for macOS-compatible backup/import data.

Grounding:

- macOS source: `VoiceInk/PowerMode/PowerModeConfig.swift` stores `AutoSendKey` as `none`, `enter`, `shiftEnter`, and `commandEnter`.
- macOS source: `VoiceInk/Transcription/Engine/TranscriptionPipeline.swift` performs auto-send after successful cursor paste with a short delay.
- Windows docs: Microsoft documents `SendInput` as the supported Win32 keystroke synthesis API, and virtual-key codes define Return, Shift, and Control keys.
- WinUI docs: ComboBox is appropriate for a short mutually exclusive option set.

## Requirements

- Each Windows `PowerModeRule` stores an auto-send key.
- Rule resolution exposes the active auto-send key without mutating base settings.
- Dictation calls auto-send only after text insertion succeeds.
- Auto-send failures become warnings rather than failed transcriptions.
- Settings and backup JSON use mac-compatible string values, not numeric enum values.
- The Power Mode editor exposes an `Auto-send key` selector.
- The Windows native sender maps:
  - `enter` to Return.
  - `shiftEnter` to Shift+Return.
  - `commandEnter` to Ctrl+Return.

## Non-Goals

- Per-rule Power Mode activation shortcuts.
- Low-level keyboard hooks.
- Sending keys before insertion completes.
- macOS commercial/license behavior.
