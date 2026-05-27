# Windows Settings Section Directory

## Context

The Windows Settings page already has macOS-aligned section descriptions for shortcuts, recording feedback, interface, clipboard, cleanup, privacy, general, backup, and diagnostics. Those descriptions were only rendered near each control group, so the top of Settings did not provide a scan-friendly directory before the dense form controls.

Microsoft WinUI `NavigationView` documentation emphasizes adaptable navigation, and UI Automation guidance requires meaningful names for important repeated UI elements. A compact Settings section directory improves visual/form polish without changing settings behavior.

## Requirements

- `SettingsSectionCopy` exposes a deterministic `AccessibleName`.
- The Settings page renders a compact section directory from `SettingsSectionPresentation.Sections`.
- Directory rows show each section title and description.
- Directory rows bind `AutomationProperties.Name` to the presenter-provided accessible name.
- Existing section descriptions remain in place near the actual controls.
- No settings persistence, backup/import, diagnostics, cleanup, or shortcut behavior changes.

## Verification

- Focused Settings presenter tests cover section accessible names.
- Static XAML accessibility tests verify `SettingsSectionDirectoryListView` row name binding.
- A WinUI app build verifies the XAML and binding compile.
