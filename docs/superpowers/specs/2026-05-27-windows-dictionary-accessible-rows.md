# Windows Dictionary Accessible Rows

## Goal

Close a Dictionary accessibility polish gap by binding existing presenter-backed accessible names into the Dictionary row templates.

## Source Of Truth

- The macOS Dictionary surfaces vocabulary, replacement, summary, and rule guidance rows as readable list entries.
- The Windows Core Dictionary presenter already exposes `AccessibleName` on these row types; the WinUI templates should surface those names through UI Automation.

## Online Grounding

- Microsoft Accessibility Insights guidance for XAML ListView items recommends setting `AutomationProperties.Name` on the DataTemplate root when the generated list item has no helpful accessible name: https://learn.microsoft.com/en-us/accessibility-tools-docs/items/uwpxaml/listitem_name

## Requirements

- Add static XAML coverage for:
  - `DictionarySummaryListView`
  - `DictionaryRuleGuidanceListView`
  - `VocabularyListView`
  - `ReplacementListView`
- Bind `AutomationProperties.Name="{Binding AccessibleName}"` on each Dictionary row template root.
- Keep dictionary sorting, selection, edit/delete behavior, import/export, and presenter output unchanged.

## Acceptance

- Focused XAML accessibility tests fail before the bindings exist.
- Focused tests pass after the bindings are added.
- Full solution tests and Debug x64 build pass.
