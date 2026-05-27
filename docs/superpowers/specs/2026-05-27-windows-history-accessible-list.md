# Windows History Accessible List

## Goal

Close a History accessibility polish gap by giving the dedicated History window list rows explicit UI Automation names.

## Source Of Truth

- The original app's History list is a navigable transcript timeline; screen reader users should hear the same status/date/transcript context visible in each row.
- Windows ListView templates with composed row text should bind explicit accessible names instead of relying on `ToString()`.

## Online Grounding

- Microsoft documents that complex WinUI ListView item templates should set `AutomationProperties.Name` on the DataTemplate root so screen readers do not fall back to the data item `ToString()`: https://learn.microsoft.com/en-us/windows/apps/design/controls/item-templates-listview

## Requirements

- Add an explicit item template for `HistoryListView`.
- Bind each row's UI Automation name to a row property.
- Keep existing history loading, selection, search, export, retry, playback, and delete behavior unchanged.
- Add focused static tests for the History window XAML and row accessible-name plumbing.

## Acceptance

- Focused accessibility tests fail before the History row template/name plumbing.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
