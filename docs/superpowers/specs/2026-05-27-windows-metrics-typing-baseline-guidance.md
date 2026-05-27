# Windows Metrics Typing Baseline Guidance

## Goal

Make the Windows Metrics dashboard explain the local typing baseline used for time-saved and keystroke-saved estimates.

## Source Of Truth

- The macOS Metrics view estimates typing time with a 35 words-per-minute baseline.
- The Windows Core and SQLite metrics aggregators already use a 35 WPM typing baseline and 5 keystrokes per word.
- Windows app data guidance treats local app data as app-owned storage; VoiceInk metrics must remain local and non-telemetry.

## Requirements

- Add a Metrics data guidance row for the typing baseline.
- State that time saved compares dictated words against 35 WPM and subtracts recorded audio duration.
- State that keystrokes saved use a 5-keystrokes-per-word estimate.
- Keep the row presenter-backed and covered by Core tests.
- Do not change metrics calculations.

## Non-Goals

- No telemetry.
- No new metrics storage schema.
- No configurable baseline in this slice.
