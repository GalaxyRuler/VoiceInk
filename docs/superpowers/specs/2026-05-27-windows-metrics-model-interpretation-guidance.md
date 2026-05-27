# Windows Metrics Model Interpretation Guidance

## Goal

Make the Metrics dashboard explain that model performance rows are local averages computed from completed records in the selected filter.

## Source Of Truth

The macOS Metrics view presents local productivity and model performance insights. The Windows fork must keep this open-source and privacy-preserving: no commercial telemetry, no remote analytics, and no hidden upload path. Users should understand that model rows are local summaries, not external benchmarking or telemetry.

## Requirements

- The dashboard data guidance list must include a `Model Performance` row.
- The row value must be `Local averages`.
- The row detail must say the model rows summarize completed local records in the selected filter, not remote telemetry.
- The guidance must be presenter-backed so the existing WinUI Metrics list renders it.

## Non-Goals

- This slice does not change metric aggregation formulas.
- This slice does not add telemetry or remote diagnostics.
