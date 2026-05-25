# Windows Soniox Transcription Design

Date: 2026-05-26

## Purpose

Add the macOS app's Soniox V4 cloud transcription provider to Windows for post-recording transcription.

## Source Of Truth

The macOS app exposes `SonioxProvider` with provider key `Soniox`, default model `stt-async-v4`, automatic language detection, and streaming capability. Soniox's current async STT docs use `https://api.soniox.com/v1`, bearer authentication, local file upload to `/v1/files`, create transcription via `/v1/transcriptions`, poll `/v1/transcriptions/{id}`, fetch `/v1/transcriptions/{id}/transcript`, and render final transcript text from token `text` fields.

## Behavior

- Add a Soniox transcription preset:
  - id `soniox`;
  - display name `Soniox`;
  - endpoint `https://api.soniox.com/v1/transcriptions`;
  - default model `stt-async-v4`;
  - model list containing `stt-async-v4`.
- Store the API key under `VoiceInk.Windows.Transcription.OpenAICompatible.Soniox.ApiKey`.
- Route Soniox through a provider-specific batch adapter because the API is async and token-based.
- Upload local audio, create transcription, poll completion, fetch transcript tokens, render token text, and best-effort delete the created transcription and uploaded file.
- Keep errors sanitized and never include response bodies or API keys in exception messages.

## Non-Goals

- Do not add Soniox realtime streaming in this slice.
- Do not expose advanced Soniox context/diarization/language-identification controls yet.
- Do not require a Soniox API key for automated verification.

## Verification

- Failing tests first for catalog, secret naming, request sequence, token rendering, cleanup, sanitized errors, and router selection.
- Focused Core and Infrastructure tests.
- Full solution tests and Debug x64 build before commit.
