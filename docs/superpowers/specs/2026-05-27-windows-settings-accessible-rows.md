# Windows Settings Accessible Rows

## Goal

Improve Settings page polish by giving repeated Settings action, preference, backup, and diagnostics rows stable accessibility names.

## Source Of Truth

- `VoiceInk/Views/Settings/SettingsView.swift` presents Settings as grouped, scan-friendly rows for repeated daily use.
- Windows already maps this content through `SettingsSectionPresenter`, so row names should be presenter-backed and testable.

## Online Grounding

- Microsoft documents WinUI `AutomationProperties.Name` as the attached-property surface for UI Automation names: https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.automation.automationproperties

## Requirements

- Add computed `AccessibleName` values to Settings action, preference, backup, and diagnostics guidance rows.
- Bind the visible Settings ListView row templates to `AutomationProperties.Name`.
- Keep visible text, settings behavior, persistence, backup/import, cleanup, and diagnostics export behavior unchanged.

## Acceptance

- Core Settings presenter tests assert representative accessible names for all four row types.
- Visible Settings row templates bind to `AccessibleName`.
