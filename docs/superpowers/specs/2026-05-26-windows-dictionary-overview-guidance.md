# Windows Dictionary Overview Guidance Spec

## Goal

Add a concise Dictionary overview that explains how vocabulary and replacements affect dictation, including disabled replacements and local JSON import/export.

## Source Of Truth

- `VoiceInk/Views/Dictionary/*`
- `VoiceInk/Services/WordReplacementService.swift`
- VoiceInk public docs for personal dictionary and word replacements.

## Requirements

- Keep presentation logic in `VoiceInk.Windows.Core`.
- Summarize vocabulary count, enabled replacement count, disabled replacement count, and local backup/import behavior.
- Preserve existing add/edit/remove/import/export behavior.
- Keep all dictionary data local and avoid commercial surfaces.

## Acceptance Criteria

- Core tests cover empty, active, and disabled replacement states.
- The Dictionary page displays the overview above the existing action buttons.
- Full solution tests and Debug x64 build pass.
