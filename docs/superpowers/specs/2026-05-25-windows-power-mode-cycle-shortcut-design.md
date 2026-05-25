# Windows Power Mode Cycle Shortcut Design

## Purpose

VoiceInk for Windows should let users switch explicit Power Mode selection from the keyboard without opening the main window or floating recorder controls.

## Desired Behavior

- Settings includes an optional `Cycle Power Mode` global shortcut.
- Blank means disabled, matching other optional shortcuts.
- The shortcut participates in existing parsing, normalization, registration, and duplicate detection.
- Pressing the shortcut cycles through:
  1. Automatic
  2. Each enabled Power Mode rule in current rule order
  3. Back to Automatic
- Disabled rules are skipped.
- If the current selected rule is missing or disabled, the shortcut treats the current state as Automatic and moves to the first enabled rule.
- If no Power Mode rules are enabled, the shortcut keeps Automatic and reports `Power Mode: Auto`.
- The shortcut can be used while recording, matching the existing floating recorder Power Mode behavior.

## Non-Goals

- No per-rule direct shortcuts in this slice.
- No shortcut recorder UI.
- No auto-send keys.

## Verification

- Core tests cover shortcut registration and duplicate detection.
- Core tests cover cycling Automatic, enabled rules, disabled/missing selections, and no-enabled-rule fallback.
- Full tests and Debug x64 build must pass.
