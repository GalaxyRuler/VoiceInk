# Windows Metrics Reset Design

Date: 2026-05-25

## Goal

Add an open-source, local-only Metrics reset control to the Windows app so users can clear accumulated session metrics without touching transcription History, settings, audio files, models, or diagnostics.

## Source Of Truth

- macOS parity area: `VoiceInk/Views/Metrics/*` and local session metrics behavior.
- Windows current state: local SQLite `metrics.db`, Metrics sidebar dashboard, time filters, model performance lists, and CSV export.
- External grounding: `Microsoft.Data.Sqlite` non-query commands are executed with `ExecuteNonQuery` / `ExecuteNonQueryAsync`; WinUI `ContentDialog` supports primary/secondary confirmation flows.

## Behavior

- The Metrics section includes a `Reset Metrics` button near Refresh and Export.
- Pressing Reset shows a confirmation dialog explaining that local metrics will be deleted and History will remain untouched.
- Canceling the dialog leaves metrics unchanged and updates status with a canceled message.
- Confirming clears all rows from the local `session_metrics` table, refreshes the current Metrics view, and updates status to `Metrics reset`.
- If Metrics are disabled for the session, Reset shows the existing disabled warning and does not attempt to clear storage.
- Reset is disabled while another app operation is active, matching Refresh and Export.

## Data And Privacy

- Reset is local-only.
- Reset does not upload telemetry.
- Reset does not alter API keys, provider settings, models, History records, audio files, or app settings.
- Reset uses the existing metrics store abstraction so Core remains UI-independent and SQLite remains in Infrastructure.

## Testing

- Add an `ISessionMetricStore.ClearAsync` contract.
- `DisabledSessionMetricStore.ClearAsync` is a no-op.
- `SqliteSessionMetricStore.ClearAsync` deletes all `session_metrics` rows.
- Tests cover clearing summary totals, transcription model performance, enhancement model performance, duplicate detection, and future saves after reset.

## Non-Goals

- No automatic retention/deletion policy in this slice.
- No batch reset of History or diagnostic files in this slice.
- No commercial analytics or telemetry surfaces.
