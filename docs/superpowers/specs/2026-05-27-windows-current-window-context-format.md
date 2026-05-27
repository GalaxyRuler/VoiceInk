# Windows Current Window Context Format

## Goal

Format Windows OCR-derived current-window context like the macOS screen-capture context so enhancement prompts carry the same terminology and structure.

## Source Of Truth

- `VoiceInk/Services/ScreenCaptureService.swift` renders screen context as `Active Window`, `Application`, and `Window Content`.
- `VoiceInk/Services/AIEnhancement/AIEnhancementService.swift` wraps that captured text in `<CURRENT_WINDOW_CONTEXT>`.
- Windows already captures active-window process/title separately and now uses the same `<CURRENT_WINDOW_CONTEXT>` tag for OCR text.

## Requirements

- When OCR text is present, render `<CURRENT_WINDOW_CONTEXT>` with:
  - `Active Window: <title>` when available;
  - `Application: <process>` when available;
  - a blank line before `Window Content:`;
  - OCR text under `Window Content:`.
- Preserve existing active-window and browser URL context sections for Windows-specific matching/debuggability.
- Keep skipping `<CURRENT_WINDOW_CONTEXT>` when OCR text is empty.

## Non-Goals

- No OCR capture changes.
- No provider request changes beyond prompt text formatting.
- No UI changes.
