# Windows Power Mode Accessible Rows

## Goal

Close a Power Mode accessibility polish gap by giving rule rows presenter-backed accessible names and binding those names in WinUI.

## Source Of Truth

- The macOS app presents Power Mode rules as rich rows that summarize name, targets, overrides, shortcuts, and enabled state.
- Windows should expose that same row meaning as a single UI Automation name while preserving the visible row layout.

## Online Grounding

- Microsoft Accessibility Insights guidance for XAML ListView items recommends setting `AutomationProperties.Name` on the DataTemplate root when generated list items do not expose helpful accessible names: https://learn.microsoft.com/en-us/accessibility-tools-docs/items/uwpxaml/listitem_name

## Requirements

- Add `AccessibleName` to `PowerModeRuleRowPresentation`.
- Build the accessible name from title, status badge, target summary, override summary, and shortcut summary while skipping blanks.
- Bind `PowerModeRulesListView` row template to `AccessibleName`.
- Keep rule editing, selection, ordering, matching, and visual row content unchanged.

## Acceptance

- Focused presenter/XAML tests fail before the Core property and XAML binding exist.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
