# Windows xAI Transcription Design

## Goal

Port the macOS xAI Grok speech-to-text provider into the Windows fork with a provider-specific batch adapter and no commercial app surfaces.

## Source Of Truth

- macOS provider: `VoiceInk\Transcription\Cloud\XAIProvider.swift`
- xAI speech-to-text docs: batch transcription uses `POST https://api.x.ai/v1/stt`.
- xAI speech-to-text docs: authentication uses bearer API keys.
- xAI speech-to-text docs: requests include multipart audio `file`, optional `language`, and optional `format`.

## Windows Behavior

- Add an `xAI` cloud transcription preset.
- Use provider id `xai`.
- Use default model `grok-stt`.
- Store credentials under `VoiceInk.Windows.Transcription.OpenAICompatible.xAI.ApiKey`.
- Submit recorded WAV audio as multipart form data to the xAI STT endpoint.
- Send `model: grok-stt`, `format: true`, and language when it is not `auto`.
- Parse returned transcript text from JSON.
- Surface sanitized provider errors only.

## Deliberate Deferrals

- xAI realtime streaming remains a later live-preview slice.
- Provider test-call UI remains part of richer cloud model card parity.
