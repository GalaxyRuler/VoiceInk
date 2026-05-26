# Windows Metrics Action Summary

## Goal

Add a local Metrics action summary for filter scope, CSV export, model performance, and reset behavior.

## Source Of Truth

VoiceInk history and metrics help users review, reuse, analyze, and export local transcription activity. The Windows fork already records metrics, filters the dashboard, exports CSV, shows model performance, and resets metrics with confirmation. This slice makes those actions easier to scan.

## Behavior

- Extend the Core metrics dashboard presentation with action rows.
- Rows summarize:
  - Active filter scope.
  - CSV export readiness.
  - Model performance availability.
  - Reset scope.
- Keep metrics storage, CSV export, model performance queries, reset confirmation, and privacy behavior unchanged.
- Keep all metrics local unless the user exports a file.

## UI

Render action rows on the Metrics page near the existing diagnostics rows.

## Testing

Add presenter tests for populated and empty dashboards.

## Out Of Scope

- No new metrics schema.
- No telemetry.
- No analytics upload.
