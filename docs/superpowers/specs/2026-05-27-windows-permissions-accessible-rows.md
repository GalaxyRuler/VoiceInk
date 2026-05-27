# Windows Permissions Accessible Rows

## Goal

Close a Permissions accessibility polish gap by giving readiness checklist rows presenter-backed accessible names and binding those names in WinUI.

## Source Of Truth

- The macOS app treats permissions/readiness as a first-run and settings-adjacent checklist.
- Windows should expose each readiness card as a single coherent UI Automation name, including title, status, description, and fallback guidance.

## Online Grounding

- Microsoft Accessibility Insights guidance for XAML ListView items recommends setting `AutomationProperties.Name` on the DataTemplate root when generated list items do not expose helpful accessible names: https://learn.microsoft.com/en-us/accessibility-tools-docs/items/uwpxaml/listitem_name

## Requirements

- Add `AccessibleName` to `PermissionReadinessItem`.
- Build the accessible name from title, status, description, and fallback guidance while skipping blanks.
- Bind `PermissionsChecklistListView` row template to `AccessibleName`.
- Keep permissions readiness logic, action routing, and visual layout unchanged.

## Acceptance

- Focused presenter/XAML tests fail before the Core property and XAML binding exist.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
