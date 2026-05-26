# Windows Metrics Dashboard Visual Parity Spec

Last updated: 2026-05-26

## Goal

Move the Windows Metrics page closer to the macOS VoiceInk dashboard by replacing the plain summary text with a testable hero summary and card presentation.

## Source of Truth

- `VoiceInk/Views/Metrics/MetricsContent.swift`
- `VoiceInk/Views/Metrics/MetricCard.swift`

The macOS dashboard presents:

- A hero line centered on saved time with VoiceInk.
- A subtitle summarizing dictated words and session count.
- Four metric cards:
  - Sessions Recorded
  - Words Dictated
  - Words Per Minute
  - Keystrokes Saved
- Empty state text: `No Recorder Sessions Yet` and `Start your first recording to unlock value insights.`

## Windows Design

- Add a Core presenter that formats metrics into UI-independent hero/card records.
- Keep metric aggregation unchanged.
- Keep WinUI code-behind responsible only for assigning presenter output to controls.
- Preserve open-source behavior; no license, promotion, or commercial panels.

## Acceptance Criteria

- The Windows Metrics page exposes the macOS-style hero title, subtitle, and four card rows.
- Empty metrics produce the macOS-style empty state text.
- Formatting is culture-aware for numbers and compact for durations.
- Unit tests cover non-empty and empty presenter states.
- Existing metrics export, reset, filters, and model performance lists continue to work.
