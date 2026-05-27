# Windows Onboarding Row Accessibility

## Goal

Make onboarding row display and accessibility text presenter-backed instead of hand-built in WinUI code.

## Source Of Truth

- VoiceInk docs describe setup as model, microphone, shortcut, and first dictation.
- The macOS app presents setup steps as scan-friendly rows.
- Windows onboarding already uses Core presenter rows for setup actions, summary rows, setup stages, and tutorial steps.

## Requirements

- Onboarding summary, action, tutorial, and stage rows expose deterministic accessible names.
- Onboarding summary, action, and tutorial rows expose display text so WinUI does not duplicate presenter string composition.
- Existing user-facing onboarding wording must remain unchanged.
- Keep the behavior in Core presenter records and tests.
