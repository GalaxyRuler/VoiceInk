# Windows ElevenLabs Advanced Options Spec

## Intent

Let Windows users configure ElevenLabs Scribe provider options without adding a new settings surface or requiring credentials during development.

## Requirements

- Treat non-secret ElevenLabs endpoint query parameters as advanced provider options.
- Send those options as multipart form fields, matching the provider's speech-to-text request shape.
- Strip the query string from the ElevenLabs request URI after mapping options into form fields.
- Preserve existing first-class settings for file upload, model, and language.
- Ignore reserved query names that would duplicate `file`, `model_id`, or `language_code`.
- Keep API keys in secure storage and continue rejecting secret-bearing endpoint query parameters through existing validation.

## Open-Source Boundary

The feature is provider plumbing for user-owned API keys. It adds no commercial defaults, telemetry, account flow, license gate, or paid upgrade prompt.

## Verification

- Focused ElevenLabs cloud transcription request-construction tests.
