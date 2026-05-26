# Windows Model Repair Action Spec

Date: 2026-05-26

## Goal

Make local Whisper model health guidance actionable from the AI Models page.

## Windows Behavior

- Extend the model health presenter with a repair action classification.
- Show an action button next to the existing model repair hint.
- For missing, invalid, empty, suspiciously small, or unselected model states, the button opens the existing `.bin` import flow.
- For ready local models, the button warms up the selected model.
- Keep existing model settings, imported model storage, download flow, and cleanup behavior unchanged.

## Open-Source Boundary

The repair action uses only local files and existing open-source GGML import/warmup paths. It adds no paid model gates, account flows, telemetry, or bundled proprietary models.
