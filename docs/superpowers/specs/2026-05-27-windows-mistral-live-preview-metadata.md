# Windows Mistral Live Preview Metadata

## Goal

Make the Mistral transcription provider card accurately describe the current Windows implementation.

## Source Of Truth

- Windows live recorder preview starts only when a provider has an `ILiveTranscriptionPreviewService` registered in the composite live-preview service.
- Mistral batch Voxtral transcription exists in Windows, but no Mistral live-preview service exists yet.

## Online Grounding

- Mistral documents a realtime Voxtral transcription SDK stream with `voxtral-mini-transcribe-realtime-2602`.
- Mistral also documents batch/streaming audio transcription endpoints separately.
- The docs are SDK-oriented for realtime audio streaming, so Windows should not claim that its recorder live-preview path is implemented until a tested transport adapter exists.
- Reference: https://docs.mistral.ai/studio-api/audio/speech_to_text/realtime_transcription

## Requirements

- Mistral provider metadata must advertise batch transcription, not Windows live recorder preview.
- Project completion docs must continue to track Mistral realtime preview as a remaining provider-specific live-preview gap.
- No changes to the existing Mistral batch transcription adapter.

## Non-Goals

- No guessed Mistral realtime WebSocket contract.
- No SDK dependency addition.
- No credentialed Mistral smoke test.
