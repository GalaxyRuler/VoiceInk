# Windows OCR Context Settings Gate Design

## Goal

Make OCR enhancement context explicitly user controlled before a real Windows screen-capture OCR reader is added.

## External Grounding

Microsoft's Windows screen capture documentation describes `Windows.Graphics.Capture` as a consent-oriented display/window capture API for desktop apps, and `Windows.Media.Ocr` recognizes text from a supplied bitmap. Because OCR requires screen content, VoiceInk for Windows should treat it as a sensitive local context source, not as an always-on enhancement input.

## Behavior

- Add `UseOcrContext` to `AppSettings`.
- Default `UseOcrContext` to `false`.
- Persist `UseOcrContext` in JSON settings and settings backup flows through the existing `AppSettings` serialization.
- The enhancement pipeline requests OCR context only when `settings.UseOcrContext` is true.
- The Enhancement page exposes a checkbox labeled `Screen OCR Context` beside the existing clipboard context control.
- The checkbox is enabled/disabled with the other enhancement controls.
- The default native OCR reader remains no-op until a later capture/OCR implementation replaces it behind this setting.

## Non-Goals

- No real screen capture picker or OCR bitmap recognition in this slice.
- No OCR language selector in this slice.
- No cloud OCR provider.

## Open-Source Privacy Rule

OCR is local-only, default-off, and does not introduce account, licensing, telemetry, or paid provider behavior.
