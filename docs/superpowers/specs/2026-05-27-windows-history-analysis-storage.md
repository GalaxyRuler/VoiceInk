# Windows History Analysis Storage Design

## Context

VoiceInk history lets users review, copy, retry, re-enhance, export, and replay previous dictations. The inline History analysis panel already summarizes words, audio duration, and enhancement state, but it did not clearly show provider timing or whether an audio file is still attached for replay.

VoiceInk's macOS history workflow emphasizes reuse and recovery. Windows should make provider and storage state visible without requiring users to inspect metadata text.

## Goal

Add two presenter-backed History analysis rows:

- Provider: selected transcription provider plus transcription duration when available;
- Audio Storage: whether audio is attached for open/replay or the history item is text-only.

## Non-Goals

- No database schema change.
- No changes to audio retention policy.
- No new export format.
- No provider retry changes.

## Testability

`HistoryAnalysisPresenter` emits the additional rows and focused Core tests cover enhanced, failed, and audio-backed history items. The existing WinUI inline History analysis list renders all rows.
