# Windows Mistral Transcription Preset Design

Date: 2026-05-26

## Purpose

Add the macOS app's Mistral/Voxtral cloud transcription option to the Windows provider catalog using the existing OpenAI-compatible multipart transcription adapter.

## Source Of Truth

The macOS app's `MistralProvider` exposes provider key `Mistral`, model `voxtral-mini-latest`, and language auto-detect. Mistral's current API docs show `https://api.mistral.ai/v1/audio/transcriptions` and `voxtral-mini-latest` for offline transcription. This fits the Windows OpenAI-compatible request shape: bearer token, multipart audio file, model, and JSON response text.

## Behavior

- Add a Mistral transcription preset:
  - id `mistral`;
  - display name `Mistral`;
  - endpoint `https://api.mistral.ai/v1/audio/transcriptions`;
  - default model `voxtral-mini-latest`;
  - model list containing `voxtral-mini-latest`.
- Store its API key under `VoiceInk.Windows.Transcription.OpenAICompatible.Mistral.ApiKey`.
- Route normal transcription through the existing `OpenAICompatibleCloudTranscriptionService`.
- Do not add Mistral streaming in this slice.

## Verification

- Failing catalog and secret-name tests first.
- Focused Core transcription catalog tests and OpenAI-compatible adapter tests.
- Full solution tests and Debug x64 build before commit.
