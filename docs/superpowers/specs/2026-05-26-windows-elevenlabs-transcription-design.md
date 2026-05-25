# Windows ElevenLabs Transcription Design

Date: 2026-05-26

## Purpose

Add the macOS app's ElevenLabs Scribe cloud transcription provider to Windows for normal post-recording transcription.

## Source Of Truth

The macOS app exposes `ElevenLabsProvider` with models `scribe_v1` and `scribe_v2`, and defaults users toward Scribe v2. ElevenLabs' current API documentation uses `POST https://api.elevenlabs.io/v1/speech-to-text`, multipart file upload, `model_id`, and an `xi-api-key` header for batch speech-to-text conversion. Realtime Scribe uses a separate WebSocket endpoint and is out of scope for this slice.

## Behavior

- Add an ElevenLabs transcription preset:
  - id `elevenlabs`;
  - display name `ElevenLabs`;
  - endpoint `https://api.elevenlabs.io/v1/speech-to-text`;
  - default model `scribe_v2`;
  - model list `scribe_v2`, `scribe_v1`.
- Store the API key under `VoiceInk.Windows.Transcription.OpenAICompatible.ElevenLabs.ApiKey`.
- Route ElevenLabs through a provider-specific batch adapter because the request uses `xi-api-key` and `model_id`, not the OpenAI-compatible bearer/model field names.
- Return provider metadata as `elevenlabs`.
- Keep errors sanitized and avoid logging or returning API keys.

## Verification

- Failing tests first for catalog, secret naming, router selection, and ElevenLabs request/response behavior.
- Focused Core and Infrastructure tests.
- Full solution tests and Debug x64 build before commit.
