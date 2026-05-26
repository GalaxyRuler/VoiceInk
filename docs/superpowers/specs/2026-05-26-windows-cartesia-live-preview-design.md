# Windows Cartesia Live Preview Design

## Goal

Add Cartesia realtime transcript preview for the floating recorder while preserving final transcription through the batch adapter.

## Source Of Truth

- macOS provider: `VoiceInk\Transcription\Cloud\CartesiaProvider.swift`
- Cartesia realtime STT docs: WebSocket endpoint is `wss://api.cartesia.ai/stt/websocket`.
- Cartesia realtime STT docs: send binary audio chunks, send `finalize` to receive buffered transcript, send `close` to finish cleanly.
- Cartesia realtime STT docs: transcript events use `type: "transcript"`, `text`, and `is_final`.
- Cartesia realtime STT docs: use `X-API-Key` and `Cartesia-Version` headers, or `cartesia_version` query parameter when headers are unavailable.

## Windows Behavior

- Start live preview only when the selected cloud provider is `cartesia`, live preview is enabled, and a Cartesia API key is stored.
- Connect with `X-API-Key` and `Cartesia-Version: 2026-03-01`.
- Use `wss://api.cartesia.ai/stt/websocket` with model, `pcm_s16le`, and the current capture sample rate.
- Send PCM16 chunks as binary WebSocket frames.
- Parse `transcript` messages and merge final plus interim text in the recorder preview.
- On completion, send `finalize`, then `close`, then close the socket.
- Treat live preview as best effort; failures must not disrupt final transcription.
