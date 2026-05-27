# Windows Diagnostics Accessible Rows

## Goal

Close the Settings/About accessibility gap by binding the About / Open Source diagnostics guidance rows to presenter-backed accessible names.

## Source Of Truth

- The free/open-source Windows fork replaces macOS commercial support/update surfaces with local-only diagnostics and About / Open Source surfaces.
- `SettingsSectionPresenter.DiagnosticsGuidanceRows` already produces local-only diagnostics guidance rows with `AccessibleName` values.

## Online Grounding

- Microsoft documents WinUI `AutomationProperties.Name` as the attached-property surface for UI Automation names: https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.automation.automationproperties

## Requirements

- Bind `DiagnosticsGuidanceListView` row templates to `AutomationProperties.Name="{Binding AccessibleName}"`.
- Add a static XAML guard test so the binding does not regress.
- Keep diagnostics export/copy/open behavior unchanged.

## Acceptance

- Focused XAML accessibility guard fails before the binding and passes after.
- Existing Settings presenter tests continue to pass.
