# Windows Model Health Guidance Design

## Context

VoiceInk for Windows already supports local Whisper catalog downloads, imported `.bin` references, default model selection, path health checks, stale import cleanup, and warmup. The model page still depended on one repair hint for the currently selected model path, while the macOS experience uses scan-friendly model status and local-first language throughout the AI Models area.

Recent Windows local-model UX guidance emphasizes clear local storage boundaries, format expectations, and actionable remediation when a model file is missing or incomplete. This slice makes the selected model's lifecycle state visible without changing model loading behavior.

## Goal

Add presenter-backed model health guidance rows:

- ready models show file usability, warmup availability, and local storage behavior;
- broken or missing models show repair, blocked warmup, and safe reference repair behavior;
- the WinUI model page renders the rows near the existing repair action.

## Non-Goals

- No new model file format.
- No checksum/signature validation yet.
- No deletion of model binaries.
- No dependency or storage schema changes.

## Testability

`LocalWhisperModelHealthPresenter` exposes the rows and Core tests cover ready and broken model states. WinUI binds the row list without adding fragile UI automation.
