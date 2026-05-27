# Windows Onboarding Automation Names

## Goal

Close an onboarding accessibility parity gap by giving first-run setup dialog controls explicit UI Automation names.

## Source Of Truth

- The macOS onboarding flow is the first user-facing setup surface and must remain understandable without visual-only context.
- The Windows dialog is created programmatically in `MainWindow.xaml.cs`, so accessibility names need to be set in code rather than XAML.

## Online Grounding

- Microsoft Windows app accessibility guidance says Windows app controls integrate with UI Automation and should expose accessible names/state for assistive technologies: https://learn.microsoft.com/en-us/windows/apps/design/accessibility/accessibility
- Microsoft accessibility testing guidance recommends confirming Narrator reads each control name, state, and type: https://learn.microsoft.com/en-us/windows/apps/design/accessibility/accessibility-testing

## Requirements

- Set explicit `AutomationProperties.Name` values on the first-run setup text boxes, combo boxes, action buttons, readiness list regions, checklist, status, and health text regions.
- Add a static guard so future refactors do not remove these programmatic names.
- Keep onboarding behavior, save/skip semantics, microphone actions, and model download behavior unchanged.

## Acceptance

- A focused accessibility test fails before the names and passes after.
- Focused onboarding accessibility coverage passes.
- Full solution tests and Debug x64 build pass.
