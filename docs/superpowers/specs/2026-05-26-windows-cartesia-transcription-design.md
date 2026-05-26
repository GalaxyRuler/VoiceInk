# Windows Cartesia Transcription Design

## Goal

Port Cartesia Ink Whisper transcription into the Windows fork as a provider-specific batch adapter, while keeping realtime preview as a separate later slice.

## Source Of Truth

- macOS provider: `VoiceInk\Transcription\Cloud\CartesiaProvider.swift`
- Cartesia batch STT docs: `POST https://api.cartesia.ai/stt`.
- Cartesia batch STT docs: authentication uses `Authorization: Bearer <token>`.
- Cartesia batch STT docs: requests require the `Cartesia-Version` header, currently `2026-03-01`.
- Cartesia batch STT docs: multipart form includes audio `file`, `model`, optional `language`, and returns JSON `text`.

## Windows Behavior

- Add a `Cartesia` cloud transcription preset.
- Use provider id `cartesia`.
- Use default model `ink-whisper`.
- Store credentials under `VoiceInk.Windows.Transcription.OpenAICompatible.Cartesia.ApiKey`.
- Submit recorded WAV audio as multipart form data.
- Send `Cartesia-Version: 2026-03-01`.
- Send explicit language when it is not `auto`; omit language for app auto mode because Cartesia defaults to `en` and does not list `auto`.
- Parse JSON `text`.
- Surface sanitized provider errors only.

## Deliberate Deferrals

- Realtime Cartesia preview remains a later WebSocket slice.
- Word timestamps and duration metadata remain future history/analysis work.
