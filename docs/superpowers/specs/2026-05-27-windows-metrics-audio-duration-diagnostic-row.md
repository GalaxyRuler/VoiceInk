# Windows Metrics Audio Duration Diagnostic Row Design

## Context

VoiceInk for Windows metrics already show total audio duration near the dashboard hero and include local diagnostics rows for source, privacy, export, and reset behavior. The diagnostics list is the scannable row-based surface used throughout the Windows fork, but it did not repeat the selected-filter audio duration there.

WinUI list guidance favors structured item rows with stable bindings. The Windows metrics dashboard should expose total recorded audio duration as row data, not only as standalone text.

## Goal

Add a diagnostics row to `SessionMetricsDashboardPresenter`:

- title `Audio Duration`;
- detail `Recorded audio in this filter totals <duration>.`;
- status badge `Local`;
- duration formatting uses the existing dashboard duration formatter.

## Non-Goals

- No metrics schema changes.
- No CSV format changes.
- No recalculation changes.
- No telemetry or cloud upload.

## Testability

Focused dashboard presenter tests cover the audio-duration diagnostics row in the non-empty metrics state.
