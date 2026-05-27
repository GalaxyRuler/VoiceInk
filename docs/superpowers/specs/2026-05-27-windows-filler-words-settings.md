# Windows Custom Filler Words Settings

## Purpose

Match the macOS filler-word manager more closely by letting Windows users edit the words removed during transcript cleanup.

## Source of Truth

- `VoiceInk/Transcription/Processing/FillerWordManager.swift` stores `FillerWords`, defaults to `uh`, `um`, `hmm`, and related variants, normalizes added words to trimmed lowercase text, and avoids duplicate entries.
- The Windows Core cleanup pipeline already supports custom filler words through `TextPostProcessingOptions.FillerWords`, but `AppSettings` and the shell did not persist or expose that list.

## Requirements

- Persist custom filler words in `AppSettings` and JSON settings/backup export.
- Keep an empty custom list as "use built-in defaults" so older settings keep current cleanup behavior.
- Normalize settings UI input by trimming, lowercasing, removing empty entries, and deduplicating case-insensitively.
- Wire custom filler words into recorder dictation, audio-file transcription, and history retry cleanup.
- Expose a simple Windows-native multiline field next to the existing Remove filler words setting.
- Do not change punctuation cleanup, dictionary replacement, text formatting, enhancement, or Power Mode override behavior.

## Non-Goals

- Do not add a separate macOS-style word-chip editor in this slice.
- Do not add commercial gating or cloud sync.
- Do not change the built-in default filler-word list.
