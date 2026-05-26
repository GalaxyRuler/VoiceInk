# Windows Model Health Guidance Spec

Last updated: 2026-05-26

## Goal

Improve local model lifecycle polish by showing actionable repair guidance for selected Whisper model health states.

## Source of Truth

- `VoiceInk/Views/AI Models/*`
- `VoiceInk/Views/ModelSettingsView.swift`
- VoiceInk docs for custom local Whisper models.

## Windows Design

- Keep existing local model health detection and action gating.
- Add a Core presenter that maps health status to user-facing guidance.
- Show the guidance under the default model status on the AI Models page.

## Acceptance Criteria

- Ready model paths show warmup-ready guidance.
- Missing/empty/suspiciously-small model paths recommend re-importing or downloading a GGML `.bin`.
- Invalid extension recommends choosing a whisper.cpp `.bin`.
- No selected model recommends downloading or importing a model.
- Presenter behavior is covered by focused Core tests.
