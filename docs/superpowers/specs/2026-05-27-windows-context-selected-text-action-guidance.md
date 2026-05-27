# Windows Context Selected Text Action Guidance

## Goal

Make the Windows Context Awareness section show an explicit action row for selected-text context, including its best-effort clipboard fallback.

## Source Of Truth

- The macOS app exposes selected text as contextual input for enhancement prompts.
- Windows can expose selected text through UI Automation text patterns when the focused app supports it.
- The Windows implementation also includes a best-effort clipboard fallback that restores the previous clipboard snapshot and must not break dictation when unavailable.

## Requirements

- Add a presenter-backed Context Awareness action row for Selected Text.
- The row must explain that selected text is automatic and uses clipboard fallback when Windows/app APIs allow it.
- Keep the existing privacy rows unchanged.
- Cover the new action row with Core tests.
- Do not change selected text capture behavior.

## Non-Goals

- No new OCR behavior.
- No persistent selected-text storage.
- No clipboard history integration.
