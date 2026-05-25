# Windows Screen OCR Reader Design

## Goal

Replace the default no-op OCR context reader with a local Windows screen OCR reader that only runs when the existing `UseOcrContext` settings gate requests OCR context.

## External Grounding

Microsoft documents `Windows.Graphics.Capture` for consent-oriented screen/window capture and `Windows.Media.Ocr` for text recognition from a `SoftwareBitmap`. For this first implementation, VoiceInk uses a conservative desktop screenshot path available to the native Windows project and converts the captured bitmap into the `SoftwareBitmap` input expected by `Windows.Media.Ocr`.

## Behavior

- `WindowsEnhancementContextProvider` uses a real local OCR reader by default.
- The OCR reader captures the virtual desktop as a PNG snapshot.
- The recognizer decodes the PNG into a `SoftwareBitmap`.
- The recognizer runs `OcrEngine.TryCreateFromUserProfileLanguages()`.
- If OCR is unsupported, unavailable, or returns no text, the reader returns an empty string.
- The provider continues to catch OCR failures and degrade to empty context.
- OCR text is capped to the same bounded-context spirit as clipboard and selected text.
- OCR still does not run unless `EnhancementContextRequest.IncludeOcr` is true, which is controlled by `AppSettings.UseOcrContext`.

## Non-Goals

- No capture picker UI in this slice.
- No per-window capture or OCR region selection in this slice.
- No cloud OCR or paid OCR provider.

## Privacy

OCR is local-only and default-off through `UseOcrContext`. Captured pixels are not persisted by this reader; only recognized text flows into the enhancement prompt when the user explicitly enables OCR context.
