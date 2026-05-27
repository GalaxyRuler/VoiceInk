# Windows Dictionary Accessible Rows

## Goal

Improve Dictionary page polish by giving repeated summary, guidance, vocabulary, and replacement rows stable accessibility names.

## Source Of Truth

- `VoiceInk/Views/Dictionary/*` presents dense row-based dictionary management with vocabulary and replacement status visible at a glance.
- Windows already uses presenter-backed row records for Dictionary content, making accessible names testable without UI automation.

## Online Grounding

- Microsoft documents WinUI `AutomationProperties.Name` as the accessible name surface for UI Automation clients: https://learn.microsoft.com/en-us/windows/apps/design/accessibility/basic-accessibility-information

## Requirements

- Add computed `AccessibleName` values to Dictionary summary, rule guidance, vocabulary, and replacement rows.
- Include the row title/display text, value/status badge, and detail text in scan-friendly order.
- Bind each Dictionary ListView row template root to `AutomationProperties.Name`.
- Do not change dictionary persistence, sorting, import/export, replacement matching, or visible row text.

## Acceptance

- Core presenter tests assert representative accessible names for all four row types.
- WinUI Dictionary row templates bind to `AccessibleName`.
