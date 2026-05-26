# Windows Soniox Live Preview Design

## Goal

Add Soniox realtime transcription as a best-effort live transcript preview source for the floating recorder, while keeping final transcription on the existing async Soniox batch path.

## Grounding

- Soniox realtime docs define `wss://stt-rt.soniox.com/transcribe-websocket`.
- Soniox requires a configuration JSON message before audio, including `api_key`, model, audio format, channel count, sample rate, and optional language hints.
- Audio streams as binary WebSocket frames.
- A stream is gracefully ended by sending an empty WebSocket frame.
- Responses contain token arrays with token text and `is_final` state.

## Requirements

- Start only when live transcript preview is enabled, the selected provider is Soniox, and a Soniox API key is stored locally.
- Connect to the Soniox realtime WebSocket without storing the key in the URI or app settings.
- Send a configuration message for `stt-rt-preview`, 16 kHz mono `s16le` audio, endpoint detection, and the selected language hint when the app language is not `auto`.
- Stream recorder audio chunks as binary WebSocket frames.
- Parse token responses into preview text.
- Send an empty binary frame during completion and close/abort without disrupting final transcription.

## Non-Goals

- Replacing the final Soniox async transcription path.
- Persisting live preview text.
- Exposing advanced Soniox realtime settings such as diarization, context, or translation.
