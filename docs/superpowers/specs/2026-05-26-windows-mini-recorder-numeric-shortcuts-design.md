# Windows Mini Recorder Numeric Shortcuts Design

The macOS mini recorder supports number shortcuts while the recorder is visible: Command+1 through Command+0 selects prompt slots, and Option+1 through Option+0 selects enabled Power Mode rules.

## Windows Adaptation

- Use `Ctrl+1` through `Ctrl+0` for prompt slots.
- Use `Alt+1` through `Alt+0` for enabled Power Mode rule slots.
- Preserve Windows `Ctrl+Alt+digit` combinations for configured global shortcuts by only matching exact Ctrl-only or Alt-only digit chords.
- Use the existing low-level keyboard hook because these shortcuts are contextual to the floating recorder and should not require persisted user configuration.
- Ignore numeric shortcuts when recorder controls are not usable.
- For Power Mode, skip the `Auto` chooser row and map slots to enabled rules, matching the macOS behavior.

## Completion

Completed on 2026-05-26:

- Added testable Core numeric shortcut slot mapping.
- Added hook-level Ctrl/Alt digit detection with repeat suppression.
- Routed prompt slots to floating-recorder prompt selection.
- Routed Power Mode slots to enabled Power Mode rules.
