# Windows Current Window Context Tag Parity

## Goal

Align the Windows screen/OCR enhancement prompt section with the macOS VoiceInk prompt contract by rendering OCR-derived window context inside `<CURRENT_WINDOW_CONTEXT>` tags.

## Source Of Truth

- `VoiceInk/Services/AIEnhancement/AIEnhancementService.swift` appends screen capture context as `<CURRENT_WINDOW_CONTEXT>`.
- `VoiceInk/Services/ScreenCaptureService.swift` formats captured context with active window title, application name, and extracted window content.
- Windows currently captures OCR text separately, but the prompt tag should preserve the original app's context terminology.
- Microsoft documents Windows OCR and UI Automation as best-effort platform APIs; unavailable context should continue to degrade gracefully.

## Requirements

- Render `EnhancementContext.OcrText` inside `<CURRENT_WINDOW_CONTEXT>` tags.
- Preserve context order: active window, browser URL, current window/OCR, selected text, clipboard, vocabulary.
- Keep the `EnhancementContext.OcrText` property name and Windows capture implementation unchanged.
- Update pipeline tests so enabled OCR context produces the macOS tag.
- Do not add new settings or provider behavior.

## Non-Goals

- No new OCR capture implementation.
- No UI Automation selected-text changes.
- No storage/schema changes.
- No commercial telemetry or cloud fallback.
