# Windows Mistral Live Preview

## Goal

Add Mistral Voxtral realtime transcription to the Windows floating recorder live-preview pipeline.

## Source Of Truth

- The macOS app exposes cloud-provider recorder feedback where the provider supports streaming.
- The Windows recorder live preview starts only through `ILiveTranscriptionPreviewService`.
- Official Mistral docs and SDK source document realtime transcription over WebSocket at `/v1/audio/transcriptions/realtime`.

## Online Grounding

- Mistral docs list `voxtral-mini-transcribe-realtime-2602` as the realtime-capable transcription model and describe PCM `pcm_s16le` audio streaming.
- Mistral Python SDK builds a WebSocket URL from `/v1/audio/transcriptions/realtime`, passes `model` as a query parameter, sends `Authorization: Bearer ...`, updates `audio_format`, sends `input_audio.append`, then `input_audio.flush` and `input_audio.end`.
- Mistral realtime text arrives as `transcription.text.delta`; completion arrives as `transcription.done`.
- References:
  - https://docs.mistral.ai/studio-api/audio/speech_to_text/realtime_transcription
  - https://github.com/mistralai/client-python/blob/main/src/mistralai/extra/realtime/transcription.py
  - https://github.com/mistralai/client-python/blob/main/src/mistralai/extra/realtime/connection.py

## Requirements

- Mistral provider card must advertise recorder realtime preview once the Windows service is registered.
- Mistral live preview must use the stored Mistral API key only through the existing secret store.
- The realtime URL must derive from the configured Mistral endpoint while targeting `/v1/audio/transcriptions/realtime`.
- The batch model `voxtral-mini-latest` must map to `voxtral-mini-transcribe-realtime-2602` for the realtime path without exposing that realtime model as a batch default.
- Audio chunks must be sent as JSON `input_audio.append` messages with base64 PCM bytes.
- Completion must send `input_audio.flush`, `input_audio.end`, then close the socket.
- `transcription.text.delta` messages must accumulate into a readable live partial transcript.
- `transcription.done` must publish the final server text when present.

## Non-Goals

- No credentialed cloud smoke test without a user-owned Mistral API key.
- No new SDK dependency.
- No commercial telemetry or vendor account flows.
