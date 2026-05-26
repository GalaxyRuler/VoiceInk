# Windows Onboarding Summary Spec

Date: 2026-05-26

## Goal

Make the Windows first-run setup dialog easier to scan by adding a compact readiness summary that mirrors VoiceInk's setup basics: model, microphone, shortcut, and first dictation.

## Windows Behavior

- Extend the Core onboarding presenter with summary rows.
- Show summary rows in the first-run dialog above the detailed checklist.
- Rows must describe Model, Microphone, Shortcut, and First Dictation.
- Each row carries a short status badge: Ready, Required, Check, or Try next.
- Keep existing onboarding save, skip, model download, microphone refresh, and shortcut behavior unchanged.

## Open-Source Boundary

No commercial onboarding, trial prompts, account setup, telemetry opt-ins, or paid upgrade prompts are added. Setup remains local-first and source-runnable.
