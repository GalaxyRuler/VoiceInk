# Windows Metrics Accessible Dashboard

## Goal

Close a Metrics accessibility gap by giving dashboard cards, data guidance rows, action rows, and diagnostics rows stable UI Automation names.

## Source Of Truth

- Metrics must remain local-only and readable without visual layout context.
- The XAML already binds some Metrics templates to `AccessibleName`; the presenter needs to provide those names consistently for every repeated dashboard row type.

## Online Grounding

- Microsoft documents `AutomationProperties.Name` as the human-readable UI Automation identifier most often reported by assistive technologies: https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.automation.automationproperties

## Requirements

- Add computed accessible names for Metrics dashboard cards, data guidance rows, action rows, and diagnostics rows.
- Bind Metrics action and diagnostics row templates to `AutomationProperties.Name`.
- Add focused tests for presenter names and XAML bindings.
- Keep Metrics totals, filtering, CSV export, reset semantics, and local-only privacy text unchanged.

## Acceptance

- Focused tests fail before the presenter names and missing bindings.
- Focused Metrics/accessibility tests pass after implementation.
- Full solution tests and Debug x64 build pass.
