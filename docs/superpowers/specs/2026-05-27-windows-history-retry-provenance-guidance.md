# Windows History Retry Provenance Guidance Spec

Date: 2026-05-27

## Context

VoiceInk for Windows already has a dedicated History window with retry, re-enhance, copy, paste, playback, CSV export, and local analysis rows. The remaining parity gap for this slice is clearer provenance: when a user selects a history item, the analysis panel should explain what Retry and Re-enhance will reuse.

Windows privacy and diagnostic guidance emphasizes user-initiated export and understandable local records. VoiceInk should keep History local by default while making the reuse behavior explicit before users trigger actions.

## Requirements

- The History analysis presenter must include a `Retry and Re-enhance` row for every selected item.
- For completed rows with saved audio and original text, the row must explain that Retry uses saved audio and Re-enhance uses original transcript text.
- For completed rows without saved audio, the row must explain that Retry needs saved audio while Re-enhance can still use available transcript text.
- For failed or canceled rows, the row must say Retry and Re-enhance are unavailable because the item is not completed.
- The behavior must live in core presenter output and be covered by unit tests.

## Non-Goals

- Do not change retry or re-enhancement execution behavior.
- Do not add telemetry or external diagnostics.
- Do not change CSV export schema in this slice.
- Do not alter history retention or privacy cleanup behavior.

## Acceptance Criteria

- `HistoryAnalysisPresenter.Present` returns a `Retry and Re-enhance` row before `Export Scope`.
- Existing analysis tests cover enhanced, failed, and audio-backed completed rows.
- Project completion documentation reflects the history provenance slice.
- Focused tests, full solution tests, Debug x64 build, and whitespace validation pass.
