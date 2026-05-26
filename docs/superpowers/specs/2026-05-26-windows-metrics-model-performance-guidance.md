# Windows Metrics Model Performance Guidance Spec

## Goal

Improve the Metrics page model performance sections so transcription and enhancement model rows explain session count, speed, average processing time, and average audio/enhancement context in a macOS-aligned local diagnostics style.

## Source Of Truth

- `VoiceInk/Views/Metrics/*`
- `VoiceInk/Models/SessionMetrics.swift`
- VoiceInk public docs for transcription history review, reuse, and analysis.

## Requirements

- Keep model performance presentation in `VoiceInk.Windows.Core`.
- Preserve existing SQLite metrics storage and CSV export behavior.
- Show empty guidance when no model stats exist.
- Present transcription rows with speed factor, average processing duration, average audio duration, and session count.
- Present enhancement rows with average processing duration and session count.
- Keep all metrics local-only and avoid commercial telemetry language.

## Acceptance Criteria

- Core tests cover transcription rows, enhancement rows, and empty states.
- The Metrics page uses presenter rows instead of ad hoc UI string formatting.
- Full solution tests and Debug x64 build pass.
