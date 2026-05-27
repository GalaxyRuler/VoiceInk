# Windows Metrics Accuracy Boundary

## Goal

Clarify that the Windows Metrics dashboard reports local usage and productivity estimates, not transcription accuracy or WER benchmarks.

## Source Of Truth

- The macOS metrics dashboard presents sessions, words, words per minute, keystrokes saved, and model performance as local productivity insights.
- Windows already records local metrics in SQLite and model-performance summaries from completed records.
- Users may compare transcription models, so the UI must not imply that metrics are measuring transcription correctness.

## Requirements

- Add a presenter-backed Metrics data guidance row for accuracy boundaries.
- The row must state that metrics summarize usage, speed, and saved effort.
- The row must state that metrics do not score transcription accuracy or replace checking History output.
- Keep the behavior in Core presenter logic and existing WinUI bindings.
- Do not add telemetry, cloud analytics, commercial analytics, or paid gates.
