# Windows Onboarding Health Summary

## Context

The onboarding dialog now explains model selection, microphone readiness, shortcuts, text insertion, optional context awareness, and first dictation. Its compact health summary still omitted the text-insertion and context-awareness readiness lines, so users could read the lower action rows without seeing those checks in the health block.

## Requirements

- Onboarding health summary includes a text-insertion line that confirms the focused-field and clipboard paste path is explained.
- Onboarding health summary includes a context-awareness line that confirms context is optional and off by default during setup.
- These rows are informational only and do not change setup completion requirements.
- No Windows privacy setting, registry setting, certificate store, or global machine configuration is mutated.

## Verification

- Focused onboarding setup-status tests cover the new health summary rows.
- Existing onboarding presenter tests continue to cover the visible action, summary, tutorial, and current-step flow.
