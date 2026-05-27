# Windows Metrics Diagnostics Export Guidance Spec

Date: 2026-05-27

## Context

VoiceInk for Windows already records local session metrics, displays dashboard cards, exports CSV, and supports reset without deleting History. The remaining parity gap for this slice is clearer diagnostics language that separates VoiceInk's local metrics export from Windows' own Diagnostic Data Viewer export.

Microsoft's Windows Diagnostic Data Viewer provides a user-initiated CSV export for Windows diagnostic events that may be sent to Microsoft. VoiceInk's Metrics export is different: it is local app data from completed VoiceInk sessions, exported only when the user asks.

## Requirements

- The Metrics dashboard diagnostics rows must include a `Windows Diagnostics` row.
- The row must state that VoiceInk metrics export is separate from Windows Diagnostic Data Viewer exports.
- The row must be core presenter data with unit-test coverage.
- The change must not add telemetry, OS diagnostic access, or Windows diagnostic data export integration.

## Non-Goals

- Do not change the CSV schema.
- Do not read Windows diagnostic data.
- Do not change metrics retention or reset behavior.
- Do not add a new dependency.

## Acceptance Criteria

- `SessionMetricsDashboardPresenter.Present` returns the `Windows Diagnostics` diagnostics row between `Export` and `Reset`.
- Focused Metrics dashboard presenter tests cover the row title, detail, and status badge.
- Project completion documentation reflects the metrics diagnostics slice.
- Focused tests, full solution tests, Debug x64 build, and whitespace validation pass.
