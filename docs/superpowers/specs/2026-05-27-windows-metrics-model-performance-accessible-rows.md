# Windows Metrics Model Performance Accessible Rows

## Goal

Close a Metrics accessibility polish gap by binding the existing model-performance row accessible names into the WinUI templates.

## Source Of Truth

- The macOS app exposes model performance as readable diagnostic rows.
- The Windows Core presenter already builds `ModelPerformanceRow.AccessibleName`; the WinUI templates should surface that value to UI Automation.

## Online Grounding

- Microsoft documents that WinUI ListView item templates can set `AutomationProperties.Name` on the root element of the `DataTemplate`: https://learn.microsoft.com/en-us/windows/apps/design/controls/item-templates-listview

## Requirements

- Add static XAML coverage for both model performance lists:
  - `TranscriptionModelPerformanceListView`
  - `EnhancementModelPerformanceListView`
- Bind `AutomationProperties.Name="{Binding AccessibleName}"` on each model performance row template root.
- Keep Metrics data loading, row layout, status badges, and presenter output unchanged.

## Acceptance

- Focused XAML accessibility tests fail before the binding exists.
- Focused tests pass after the binding is added.
- Full solution tests and Debug x64 build pass.
