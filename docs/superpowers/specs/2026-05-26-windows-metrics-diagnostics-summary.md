# Windows Metrics Diagnostics Summary Spec

Date: 2026-05-26

## Goal

Make the Metrics page clearer about what is measured, where it is stored, and what the local export/reset actions do.

## Windows Behavior

- Extend the Metrics dashboard presenter with diagnostics rows.
- Show rows for data source, privacy, export, and reset/model-performance coverage.
- Keep metrics persistence, aggregation, export CSV, reset, and model-performance queries unchanged.
- Use local-only language; metrics stay on the Windows profile unless exported by the user.

## Open-Source Boundary

No telemetry, cloud analytics, paid dashboards, account metrics, or commercial diagnostics are added. The summary is derived from existing local session metrics state.
