# Windows Floating Recorder Control Accessibility Spec

## Goal

Expose dynamic UI Automation names and help text for the floating recorder Prompt and Power Mode controls.

## Source Of Truth

- The macOS mini/notch recorder uses compact icon controls for prompt and Power Mode selection.
- The Windows recorder already updates visible labels and tooltips as prompt and Power Mode selections change.
- Microsoft UI Automation guidance for buttons expects self-labeled controls and supports HelpText that describes the result of activating the control, commonly matching tooltip-style guidance.

## Requirements

- The Core floating recorder control presenter must expose:
  - prompt button accessible name
  - prompt button help text
  - Power Mode button accessible name
  - Power Mode button help text
- The prompt accessible name must include the selected prompt when enhancement is enabled and must describe the prompt chooser when enhancement is disabled.
- The Power Mode accessible name must include the selected Power Mode label or the unavailable state.
- The WinUI floating recorder must apply these names/help text whenever controls refresh.
- Existing visible labels, tooltips, popover behavior, and selection behavior must remain unchanged.

## Non-Goals

- No GUI automation on the active desktop.
- No visual redesign of the floating recorder.
- No changes to prompt or Power Mode selection persistence.
