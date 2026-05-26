# Windows Shortcut Recorder UI Design

## Goal

Make shortcut configuration feel like a recorder instead of manual text entry while preserving the existing validated capture, modifier-only recording shortcuts, and global hotkey registration behavior.

## Grounding

- Microsoft WinUI keyboard input docs support handling `KeyDown`/`PreviewKeyDown` on text controls.
- The Windows app already has Core shortcut capture/parsing validation and WinUI `KeyDown` handlers for all shortcut fields.
- The parity gap is presentation and workflow: the fields should invite capture rather than typing.

## Behavior

- Shortcut boxes become read-only capture targets.
- Each shortcut row has a `Record` button that focuses the matching capture box, selects its current value, and shows a capture status.
- Pressing a valid shortcut writes the normalized shortcut text through the existing capture logic.
- Escape still clears the focused shortcut.
- Modifier-only capture remains allowed only for primary and secondary recording shortcuts.
- Power Mode per-rule shortcuts use the same recorder affordance.

## Verification

- Debug x64 build, because this changes WinUI XAML names/events.
- Full solution tests before commit, because shortcut parsing and settings validation are shared behavior.
