# Windows History Retry Provenance Guidance Plan

Date: 2026-05-27

## Goal

Make History retry and re-enhancement provenance visible in the selected-item analysis rows.

## Steps

1. Add failing History analysis presenter assertions for a `Retry and Re-enhance` row.
2. Implement a core presenter helper that derives row value/detail from item status, audio-file presence, and original-text availability.
3. Run the focused History analysis presenter tests.
4. Update the project completion tracker and slice documentation.
5. Run the full verification gate:
   - `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
   - `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
   - `git diff --check`
6. Commit the completed slice.

## Review Notes

- Keep this informational; execution logic remains in `HistoryRetryService` and `HistoryReenhancementService`.
- Preserve the local-only export disclosure row.
- Use presenter tests instead of fragile History window automation.
