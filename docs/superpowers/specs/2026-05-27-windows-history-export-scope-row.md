# Windows History Export Scope Row Design

## Context

VoiceInk for Windows History supports list/detail views, retry, re-enhance, copy/paste actions, CSV export, audio playback/open, and local audio storage metadata. The selected-item analysis rows show words, audio duration, provider, enhancement state, and audio storage state, but did not state when history text, metadata, and audio paths leave the app.

Privacy-first transcription tools commonly distinguish local history from explicit export/copy actions. The Windows fork should make this boundary visible in the per-item analysis panel.

## Goal

Add a History analysis row:

- title `Export Scope`;
- value `User initiated`;
- detail `History text, metadata, and audio paths stay local unless you copy, paste, or export them.`;
- row appears for completed, failed, canceled, text-only, and audio-backed history items.

## Non-Goals

- No CSV schema changes.
- No history persistence changes.
- No audio retention changes.
- No telemetry or cloud sync.

## Testability

Focused History analysis presenter tests cover the export-scope row for text-only, failed, and audio-backed items.
