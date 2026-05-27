# Windows Power Mode Setup Directory Plan

## Slice

Add a scan-friendly Power Mode setup directory that explains the rule-building workflow without changing runtime behavior.

## Tasks

1. Add failing presenter and XAML accessibility coverage for setup rows.
2. Extend `PowerModePagePresenter` with presenter-backed setup rows and accessible names.
3. Render the rows in `MainWindow.xaml` and bind them from `MainWindow.xaml.cs`.
4. Update the completion tracker and run focused verification plus an app build.

## Review Notes

- Keep the rows informational only.
- Keep all stateful Power Mode logic in existing services/controllers.
- Preserve free/open-source behavior and avoid any telemetry, account, or paid surface.
