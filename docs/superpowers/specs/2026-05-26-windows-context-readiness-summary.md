# Windows Context Readiness Summary

## Goal

Add a local context readiness summary to the Enhancement page so users can see which context sources VoiceInk will use before recording.

## Source Of Truth

VoiceInk emphasizes context-aware dictation: clipboard context, selected text, active app or website context, and screen OCR context. The Windows fork already gathers those sources where available and degrades gracefully. This slice makes the current state visible without changing capture behavior.

## Behavior

- Show rows for:
  - Clipboard context.
  - Selected text context.
  - Active app and browser URL context.
  - Screen OCR context.
- Derive rows from `AppSettings`.
- Show OCR as off, full screen, configured region, or needs region.
- Keep context collection local and best-effort.
- Do not add telemetry, cloud sync, paid context features, or account flows.

## UI

Render a compact summary list near the Enhancement context controls. Each row has a title, value, detail, and status badge.

## Testing

Add Core presenter tests for:

- Default context settings.
- Enabled clipboard and full-screen OCR.
- Region-constrained OCR with a valid region.
- Region-constrained OCR with a missing region.

## Out Of Scope

- No new context capture backend.
- No OCR engine changes.
- No permissions prompts beyond existing Windows settings links and graceful degradation.
