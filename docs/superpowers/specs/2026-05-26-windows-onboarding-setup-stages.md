# Windows Onboarding Setup Stages Spec

Date: 2026-05-26

## Goal

Make first-run onboarding feel more like a guided setup flow by showing clear stages for model, microphone, shortcut, and first dictation readiness.

## Source Of Truth

VoiceInk setup guidance centers on choosing a local model, allowing microphone access, configuring the shortcut, and making the first dictation from a focused text field. The Windows fork already saves those settings; this slice improves the first-run presentation.

## Windows Behavior

- Extend `OnboardingChecklistPresenter` with setup-stage rows.
- Stages are ordered:
  1. Local model
  2. Microphone
  3. Shortcut
  4. First dictation
- Each row includes a state, title, description, and compact display text for WinUI binding.
- The dialog shows the stage rows above the detailed checklist.
- Save/skip behavior, validation, settings persistence, and microphone refresh behavior stay unchanged.

