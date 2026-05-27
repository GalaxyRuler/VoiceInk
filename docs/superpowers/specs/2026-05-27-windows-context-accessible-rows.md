# Windows Context Accessible Rows

## Goal

Close a Context accessibility polish gap by giving Enhancement context readiness, privacy, and action rows presenter-backed accessible names and binding those names in WinUI.

## Source Of Truth

- The macOS app presents context readiness and privacy information as readable local guidance rows.
- Windows should expose each row as one coherent UI Automation name instead of separate visual fragments.

## Online Grounding

- Microsoft documents that WinUI ListView item templates can set `AutomationProperties.Name` on the root element of the `DataTemplate`: https://learn.microsoft.com/en-us/windows/apps/design/controls/item-templates-listview

## Requirements

- Add `AccessibleName` to:
  - `EnhancementContextReadinessRow`
  - `EnhancementContextPrivacyRow`
  - `EnhancementContextActionRow`
- Build accessible names from title, value, status badge, and detail while skipping blanks.
- Bind `AutomationProperties.Name="{Binding AccessibleName}"` on the three Enhancement context ListView templates.
- Keep existing context text, settings behavior, OCR controls, and prompt rendering unchanged.

## Acceptance

- Focused tests fail before the Core row properties and XAML bindings exist.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
