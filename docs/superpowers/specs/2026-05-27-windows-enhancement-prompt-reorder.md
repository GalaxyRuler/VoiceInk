# Windows Enhancement Prompt Reordering

## Goal

Close an AI enhancement parity gap by letting Windows users reorder custom enhancement prompts, matching the macOS prompt grid's user-controlled prompt order in a Windows-native way.

## Source Of Truth

- The macOS `EnhancementSettingsView` uses a reorderable prompt grid backed by the persisted `customPrompts` order.
- Windows currently supports creating, editing, deleting, selecting, and trigger-word editing for prompts, but lacks a reorder action.
- WinUI ComboBox is a selection control, so Windows should expose explicit Move Up / Move Down editor buttons rather than trying to drag-reorder the ComboBox dropdown.

## Requirements

- Add Core prompt-order logic that reorders custom prompts while keeping predefined prompt positions fixed.
- Add Windows Enhancement prompt editor Move Up and Move Down buttons.
- Moving a prompt persists through the existing prompt library/settings save path and keeps the moved prompt selected.
- Predefined prompts cannot be moved; boundary moves do nothing.
- Add tests for Core reordering and static XAML/code wiring.

## Acceptance

- Focused tests fail before implementation because the reorder API and controls are missing.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
