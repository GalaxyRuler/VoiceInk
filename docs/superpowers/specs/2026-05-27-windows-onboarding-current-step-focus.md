# Windows Onboarding Current-Step Focus

## Context

The macOS onboarding flow presents setup as a guided sequence: welcome, permissions, model setup, and first dictation tutorial. The Windows fork already exposes setup actions, readiness rows, stage rows, and tutorial rows, but the first-run dialog still reads as a dense checklist rather than a focused current-step flow.

Microsoft WinUI guidance for content-heavy layouts keeps long dialog content accessible through constrained scrolling, and Microsoft UI Automation guidance expects meaningful names for important UI elements. The Windows adaptation should preserve the existing `ContentDialog` shell while adding a prominent, presenter-backed current-step summary.

## Requirements

- `OnboardingChecklistPresentation` exposes the current setup step as stable Core data:
  - step label, for example `Step 1 of 4`;
  - title;
  - description;
  - status badge;
  - UI Automation name.
- The current step is the first stage needing attention, otherwise the first advisory stage, otherwise the final ready stage.
- The WinUI onboarding dialog renders the current-step summary near the top, after the hero/progress copy and before the dense action lists.
- The current-step summary has a deterministic UI Automation name derived from the presenter.
- The change does not alter onboarding completion rules, settings persistence, microphone refresh, model download/import, or shortcut validation.

## Verification

- Focused Core presenter tests cover incomplete and ready current-step selection.
- Static WinUI tests verify the dialog creates an automation-named current-step control and binds its name from the presenter.
- Existing broader onboarding presenter tests remain green.
