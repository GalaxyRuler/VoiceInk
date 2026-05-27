# Windows ElevenLabs Live Preview

## Goal

Add live recorder transcript preview for the ElevenLabs transcription preset without changing the existing batch transcription path.

## Source Of Truth

- The macOS VoiceInk recorder exposes live/partial transcript feedback where provider capabilities allow it.
- The Windows floating recorder already shows live preview when `ShowLiveTranscriptPreview` is enabled and the selected cloud provider has an `ILiveTranscriptionPreviewService`.

## Online Grounding

- ElevenLabs documents a realtime Speech to Text WebSocket at `wss://api.elevenlabs.io/v1/speech-to-text/realtime`.
- Authentication uses the `xi-api-key` header for server-side clients.
- Audio is sent as `input_audio_chunk` JSON messages with base64 audio and `sample_rate`.
- Received transcript events include `partial_transcript`, `committed_transcript`, and `committed_transcript_with_timestamps`.
- Reference: https://elevenlabs.io/docs/api-reference/speech-to-text/v-1-speech-to-text-realtime

## Requirements

- The ElevenLabs preset advertises realtime preview.
- `CompositeLiveTranscriptionPreviewService` can start an ElevenLabs live session when:
  - live preview is enabled,
  - transcription provider is OpenAI-compatible,
  - cloud provider preset is ElevenLabs,
  - an ElevenLabs API key is present.
- The service connects with `xi-api-key`, sends JSON text audio chunks, and emits partial and committed transcript text.
- The service keeps the existing batch `ElevenLabsCloudTranscriptionService` behavior unchanged.
- Missing API keys return `null` so the recorder gracefully falls back to no live preview.

## Non-Goals

- No new account, trial, purchase, or commercial flow.
- No client-side single-use token flow.
- No provider-specific billing or premium feature copy.
- No API-key smoke against the real ElevenLabs service in automated tests.
