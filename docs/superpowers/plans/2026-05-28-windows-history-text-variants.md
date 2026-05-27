# Windows History Text Variants Plan

## Slice

Add a scan-friendly `Text Variants` analysis row so History clearly surfaces original, enhanced, and final text availability.

## Tasks

1. Add failing History analysis presenter coverage for enhanced, non-enhanced, and failed items.
2. Add a Core-presented `Text Variants` row.
3. Update the completion tracker and verify focused Core tests.

## Review Notes

- Do not change history storage, copy selectors, retry/re-enhance behavior, CSV export, audio playback, or privacy cleanup.
- Keep the row in Core so embedded and dedicated History surfaces share the same presentation.
