# Windows Model Library Overview Spec

## Goal

Improve the Windows AI Models page with a clear local model library overview so users understand downloaded/imported models, the current default model, and cleanup needs before interacting with the catalog.

## Source Of Truth

- `VoiceInk/Views/AI Models/*`
- `VoiceInk/Models/TranscriptionModelRegistry.swift`
- `VoiceInk/Transcription/*`
- VoiceInk public docs for custom local Whisper models and privacy-first local transcription.

## Requirements

- Keep model lifecycle presentation logic in `VoiceInk.Windows.Core`.
- Summarize total catalog size, downloaded/imported local model count, recommended count, default model state, and unavailable imported model count.
- Use neutral local-first language and avoid commercial surfaces.
- Do not delete model files. Existing cleanup only removes stale imported references from settings.
- Wire the overview into the AI Models page without changing the transcription pipeline.

## Acceptance Criteria

- Core tests cover empty, populated, and unavailable-import model library states.
- The AI Models page displays the overview above the catalog and updates when model choices refresh.
- Full solution tests and Debug x64 build pass.
