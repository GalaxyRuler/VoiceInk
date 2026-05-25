# Windows OCR Context Contract Design

## Goal

Add the first Windows OCR context parity slice for AI enhancement without taking a hard dependency on screen-capture permission flow or packaged OCR runtime behavior.

## External Grounding

Microsoft documents `Windows.Media.Ocr.OcrEngine` as local OCR over a `SoftwareBitmap`, with `RecognizeAsync` returning recognized text and bounding data. That means the Windows app needs two separable parts:

1. A testable VoiceInk enhancement context contract for OCR text.
2. A Windows capture/OCR implementation that can produce a bitmap later.

This slice implements the first part and a graceful native reader hook. Full screen capture picker/window capture is intentionally deferred because it has a different permission and UX surface than the existing clipboard, selection, active-window, and browser URL context readers.

## Requirements

- `EnhancementContext` includes optional OCR text.
- `EnhancementContextRequest` includes an `IncludeOcr` switch.
- `IncludeOcr` defaults to false so future real OCR readers cannot run screen OCR from incidental context requests.
- AI prompt rendering appends OCR text inside a distinct context tag when present.
- OCR context renders after active-window/browser URL context and before selected text/clipboard context.
- The enhancement pipeline requests OCR context when enhancement runs.
- Future real OCR capture must still be wired behind a deliberate local settings gate before replacing the default no-op reader.
- Windows native context collection accepts an injectable OCR reader.
- OCR reader failures degrade to empty OCR context, preserving existing enhancement behavior.
- The default native reader is a no-op until screen capture/OCR plumbing is implemented.

## Non-Goals

- No screen capture picker, desktop duplication, or window capture implementation in this slice.
- No OCR language selection UI in this slice.
- No storage of OCR text outside the existing AI prompt/request path.
- No commercial or cloud OCR provider.

## Open-Source Behavior

OCR context is local-only and optional. The placeholder reader returns empty text by default, so no new permissions, network calls, or paid surfaces are introduced.
