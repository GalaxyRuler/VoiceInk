# Windows History Analysis Overlay Spec

Date: 2026-05-26

## Goal

Add a local analysis overlay for the selected History item so users can quickly inspect transcript length, audio duration, speech rate, and enhancement state.

## Windows Behavior

- Add a Core presenter for selected history analysis rows.
- Compute word count from final/enhanced text when available, falling back to original text.
- Show audio duration and words-per-minute when audio duration is positive.
- Show enhancement status as enhanced, original only, canceled, or failed.
- Render the analysis rows in the History detail pane.
- Do not change history persistence, retry, re-enhance, copy, paste, delete, audio playback, or CSV export behavior.

## Open-Source Boundary

The overlay is local-only and derived from existing SQLite history data. It adds no telemetry, cloud analysis, account flow, paid feature gate, or commercial diagnostic upload.
