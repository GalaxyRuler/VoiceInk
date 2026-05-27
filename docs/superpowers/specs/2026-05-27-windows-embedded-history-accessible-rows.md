# Windows Embedded History Accessible Rows

## Goal

Close a History accessibility polish gap in the main app History page by giving the embedded history list and selected-item analysis rows explicit accessible names.

## Source Of Truth

- The macOS app's History timeline and detail analysis are navigable transcript surfaces.
- Windows should expose visible row meaning through UI Automation in both the dedicated History window and the embedded main-window History page.

## Online Grounding

- Microsoft WinUI ListView item-template guidance notes that templates with multiple controls can otherwise fall back to `.ToString()` and recommends setting `AutomationProperties.Name` on the DataTemplate root: https://learn.microsoft.com/en-us/windows/apps/design/controls/item-templates-listview

## Requirements

- Add `AccessibleName` to `HistoryAnalysisRow`.
- Add a main-window embedded history row model with display text and accessible name.
- Bind `HistoryListView` and `HistoryAnalysisListView` templates to `AccessibleName`.
- Keep history loading, search, selection, retry, re-enhance, playback, copy, delete, and analysis content unchanged.

## Acceptance

- Focused presenter/XAML tests fail before the accessible names and template bindings exist.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
