# Windows Dictionary Row Polish Spec

Date: 2026-05-26

## Goal

Make the Windows Dictionary page easier to scan by giving vocabulary and replacement rows presenter-backed detail/status fields instead of flat display text only.

## Source Of Truth

VoiceInk documentation describes the personal dictionary as a place to teach VoiceInk names, technical terms, and phrases used often. Word Replacements automatically replace phrases and names after transcription.

## Windows Behavior

- Extend `DictionaryPagePresenter` rows with details and status badges.
- Vocabulary rows show the vocabulary word plus a local prompt/enhancement usage detail.
- Replacement rows show original text, replacement text, enabled/disabled status, and an explicit after-transcription detail.
- WinUI renders row templates for both lists while preserving existing row order, selection by index, add/remove/edit/toggle behavior, import/export, and sorting.
- Keep all dictionary data local and exportable through the existing local JSON workflows.

