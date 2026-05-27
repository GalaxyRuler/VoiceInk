# Windows Metrics Time Saved Card Spec

## Goal

Make Windows Metrics expose Time Saved as a first-class dashboard card, matching the original VoiceInk Metrics surface where saved time is immediately visible.

## Source Of Truth

The macOS `MetricsContent` view shows the formatted time-saved value prominently and computes it from estimated typing time minus recorded audio duration. The Windows metrics pipeline already computes `SessionMetricsSummary.TimeSaved`, includes it in the hero title, and exports it to CSV, but the dashboard cards omit a scan-friendly `Time Saved` card.

## Windows Behavior

The Metrics dashboard presenter must:

- Keep existing cards for Sessions Recorded, Words Dictated, Words Per Minute, and Keystrokes Saved.
- Insert a `Time Saved` card before `Keystrokes Saved`.
- Format the card value with the existing `SessionMetricsDashboardPresenter.FormatDuration` behavior.
- Describe the value as `estimated typing time saved`.
- Keep empty-state behavior unchanged: no dashboard cards when there are no sessions.

## Open-Source Boundary

This is a local presenter/UI data change only. It introduces no telemetry, cloud metrics sync, account flow, paid dashboard, usage tracking service, or commercial analytics.

## Verification

- Add a focused Metrics presenter test that fails when the `Time Saved` card is missing.
- Run focused Metrics presenter tests.
- Run full Windows tests/build and `git diff --check` before committing.
