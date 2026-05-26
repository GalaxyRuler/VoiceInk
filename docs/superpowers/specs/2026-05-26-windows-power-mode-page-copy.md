# Windows Power Mode Page Copy Spec

Last updated: 2026-05-26

## Goal

Bring the Windows Power Mode page copy closer to macOS VoiceInk and public docs by adding header, count, and empty-state guidance.

## Source of Truth

- `VoiceInk/PowerMode/PowerModeView.swift`
- VoiceInk docs for Power Mode and quickly switching Power Modes.

## Acceptance Criteria

- Power Mode page shows `Power Modes` and `Automate your workflows with context-aware configurations.`
- Rule count distinguishes enabled and disabled rules.
- Empty state says `No Power Modes Yet` and explains creating a first mode for apps/websites.
- Existing rule matching, editing, ordering, shortcuts, and recorder selection behavior remain unchanged.
