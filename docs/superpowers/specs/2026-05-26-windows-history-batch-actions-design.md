# Windows History Batch Actions Design

## Context

The macOS dedicated History window supports selecting multiple transcriptions and then exporting or deleting the selection from a bottom toolbar. The new Windows dedicated History window currently has single-item actions only.

Online grounding: WinUI ListView supports multiple selection through `SelectionMode="Multiple"` and exposes the selected items collection. Windows can use that native control behavior with app-specific row objects that carry history ids.

## Goals

- Enable multiple selection in the dedicated History window.
- Add Select All, Clear Selection, Export Selected, and Delete Selected actions.
- Preserve single-row detail behavior by treating the first selected row as the detail item.
- Export only selected rows for Export Selected.
- Delete selected rows with one confirmation and best-effort app-owned audio cleanup.

## Non-goals

- Do not add performance analysis overlays in this slice.
- Do not add batch re-enhance or batch retry.
- Do not change the inline History page.
- Do not add commercial telemetry, paid prompts, account flows, or cloud sync.

## Architecture

Add a pure Core `HistoryWindowBatchCommandPresenter` that reports whether batch actions are enabled and the selection label. Update `HistoryWindow` to bind stable row objects to the ListView, use multiple selection, preserve selected ids during refresh, and drive batch actions from selected ids.

## Verification

- Core tests cover no rows, rows with no selection, partial selection, and all selected states.
- App project builds with the ListView selection changes.
- Full solution tests and x64 build pass before commit.
