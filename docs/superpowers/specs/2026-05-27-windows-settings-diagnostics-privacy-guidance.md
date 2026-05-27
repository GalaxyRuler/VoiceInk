# Windows Settings Diagnostics Privacy Guidance

## Goal

Clarify in Settings that VoiceInk diagnostic export is a manual local export and does not enable Windows optional diagnostic data or upload logs.

## Source Of Truth

The free/open-source Windows fork must not include commercial telemetry or private diagnostics upload. Windows has its own OS-level diagnostics settings, but VoiceInk's diagnostics surface is limited to local logs and sanitized summaries the user chooses to copy or export.

## Requirements

- Settings diagnostics guidance must include an `Optional Diagnostic Data` row.
- The row value must be `Not required`.
- The row detail must say VoiceInk diagnostics export does not enable Windows optional diagnostic data or upload logs.
- The row must be presenter-backed so the existing WinUI settings diagnostics list renders it.

## Non-Goals

- This slice does not change Windows diagnostic settings.
- This slice does not add telemetry, background upload, or automatic log submission.
