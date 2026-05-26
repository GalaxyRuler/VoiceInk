# Windows Dictionary Page Visual Parity Spec

Last updated: 2026-05-26

## Goal

Move the Windows Dictionary page closer to the macOS VoiceInk dictionary experience by adding section descriptions, counts, and empty-state-aware presentation records.

## Source of Truth

- `VoiceInk/Views/Dictionary/DictionarySettingsView.swift`
- `VoiceInk/Views/Dictionary/VocabularyView.swift`
- `VoiceInk/Views/Dictionary/WordReplacementView.swift`
- VoiceInk public docs for Personal Dictionary and Word Replacements.

The macOS page presents:

- Hero language: `Dictionary Settings` and `Enhance VoiceInk's transcription accuracy by teaching it your vocabulary`.
- Section choices:
  - `Word Replacements`: `Automatically replace specific words/phrases with custom formatted text`
  - `Vocabulary`: `Add words to help VoiceInk recognize them properly`
- Count labels:
  - `Vocabulary Words (N)`
  - replacement rows separated into Original and Replacement columns.
- Clear guidance text for empty lists.

## Windows Design

- Add a Core presenter that maps vocabulary/replacement data to UI-independent page rows and state text.
- Keep dictionary storage, sorting, import/export, quick add, add/edit/delete/toggle behavior unchanged.
- Use WinUI controls already present on the page; add concise description/count/empty-state text.
- Keep the app free/open-source with no commercial dictionary gating.

## Acceptance Criteria

- Dictionary page displays macOS-aligned hero and section descriptions.
- Vocabulary and replacement sections display count labels.
- Empty vocabulary/replacement lists show guidance text instead of a blank list.
- Replacement rows carry enabled/disabled status in their display.
- Presenter behavior is covered by focused Core tests.
