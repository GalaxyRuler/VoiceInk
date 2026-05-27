# Windows Filler Word Chip Editor

## Purpose

Bring the Windows filler-word settings surface closer to the macOS chip editor.

## Source of Truth

- VoiceInk docs describe adding a filler word through a text field, pressing Return or the add button, and removing words from chips.
- `VoiceInk/Views/Components/FillerWordsSettingsView.swift` implements the macOS UI as an add field, plus button, and removable filler-word chips.

## Requirements

- Replace the multiline filler-word settings field with an add field, add button, and removable chip-like word list.
- Pressing Enter in the add field should add the word.
- Adding must trim, lowercase, and reject duplicates using the Core helper.
- Removing must be case-insensitive and update the pending settings list.
- Persist the edited chip list through the existing Apply settings flow.
- Keep the existing cleanup pipeline, JSON settings, backup behavior, and built-in defaults unchanged.

## Non-Goals

- Do not add animated macOS chip hover effects.
- Do not save on each add/remove; keep the existing Windows Apply flow.
- Do not change filler-word matching semantics.
