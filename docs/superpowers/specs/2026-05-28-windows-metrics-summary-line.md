# Windows Metrics Summary Line

## Context

The Metrics dashboard already exposes a macOS-style hero, cards, action rows, data guidance, diagnostics, and model performance panels. The top of the dashboard moved directly from hero text into cards, so the selected filter, completed sessions, words, and audio duration were not available as one compact scan line.

## Requirements

- `SessionMetricsDashboardPresentation` exposes a `SummaryLine`.
- Non-empty summaries include the selected filter, completed session count, word count, and audio duration.
- Empty summaries state that the selected filter has no completed sessions yet.
- The WinUI Metrics dashboard renders the summary line near the hero.
- The summary line has a stable UI Automation name.
- No metrics persistence, aggregation, export, reset, model performance, or history behavior changes.

## Verification

- Focused Metrics presenter tests cover non-empty and empty summary lines.
- Static XAML/accessibility tests verify the summary TextBlock and binding.
- A WinUI app build verifies the XAML compiles.
