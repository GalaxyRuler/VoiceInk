# Windows Onboarding Setup Actions

## Goal

Add explicit first-run setup action rows so onboarding tells users what to do next for model, microphone, shortcut, and first dictation.

## Source Of Truth

VoiceInk's day-one workflow is simple: choose a local model, allow/check microphone input, configure a shortcut, then click a text field and dictate. The Windows fork already has the controls and readiness checks. This slice adds action-oriented rows to make the flow easier to follow.

## Behavior

- Extend the Core onboarding presenter with setup action rows.
- Actions cover:
  - Choose or download model.
  - Check microphone input.
  - Set shortcut.
  - Try first dictation.
- Keep the existing checklist, stages, summary rows, save behavior, skip behavior, model download, microphone refresh, and settings persistence unchanged.
- Keep onboarding local and open-source; no account, telemetry, paid onboarding, or commercial prompts.

## UI

Render action rows inside the existing first-run dialog between progress and the detailed summary/checklist. The rows can use compact text in the current code-built dialog.

## Testing

Add presenter tests for:

- Incomplete setup action statuses and command labels.
- Complete setup smoke-test action readiness.

## Out Of Scope

- No new onboarding wizard screens.
- No new persistence fields.
- No installer or permission elevation changes.
