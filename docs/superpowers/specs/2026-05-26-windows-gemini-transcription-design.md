# Windows Gemini Transcription Design

## Goal

Port the macOS Gemini cloud transcription option into the Windows fork with a provider-specific Gemini API adapter and no commercial app surfaces.

## Source Of Truth

- macOS provider: `VoiceInk\Transcription\Cloud\GeminiProvider.swift`
- Google Gemini audio docs: Gemini accepts audio input through `generateContent`, supports uploaded files and inline audio data, and can produce speech-to-text transcripts.
- Google Gemini audio docs: inline audio data is passed as base64 in an `inline_data` part.
- Google Gemini audio docs: Gemini API does not support realtime transcription through this audio path.

## Windows Behavior

- Add a `Gemini` cloud transcription preset.
- Use provider id `gemini`.
- Use default model `gemini-2.5-flash`, matching the macOS low-latency default preference.
- Include macOS model ids: `gemini-2.5-pro`, `gemini-2.5-flash`, `gemini-3.1-pro-preview`, and `gemini-3-flash-preview`.
- Store credentials under `VoiceInk.Windows.Transcription.OpenAICompatible.Gemini.ApiKey`.
- Send requests to `https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent`.
- Authenticate with `x-goog-api-key`, not a bearer header.
- Send recorded WAV bytes as an inline audio part with `mime_type: "audio/wav"`.
- Request a plain transcript by prompt and parse returned candidate text.
- Surface sanitized provider errors only.

## Deliberate Deferrals

- Files API upload for large recordings remains future work; the first Windows slice uses inline audio, matching typical short dictation recordings.
- Realtime Gemini preview is intentionally omitted because the Gemini audio docs point realtime use cases to other APIs.
- Provider test-call UI remains part of the richer cloud model card parity work.
