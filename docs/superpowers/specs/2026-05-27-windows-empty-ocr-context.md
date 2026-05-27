# Windows Empty OCR Context

## Goal

Match macOS behavior when screen capture succeeds but OCR detects no text: keep the current-window context and explicitly state that no text was detected.

## Source Of Truth

- `VoiceInk/Services/ScreenCaptureService.swift` appends `Window Content:\nNo text detected via OCR` when Vision OCR returns no text.
- Windows `WindowsScreenOcrTextReader` currently returns an empty string for empty recognizer output, causing the prompt renderer to omit `<CURRENT_WINDOW_CONTEXT>`.

## Requirements

- If capture returns no image bytes, continue returning empty text.
- If capture returns image bytes and OCR recognizer returns only whitespace, return `No text detected via OCR`.
- Preserve trimming and character cap behavior for recognized non-empty text.
- Keep this local-only and provider-independent.

## Non-Goals

- No changes to capture permissions.
- No UI changes.
- No OCR confidence reporting.
