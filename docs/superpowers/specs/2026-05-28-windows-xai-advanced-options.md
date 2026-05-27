# Windows xAI Advanced Transcription Options

## Goal

Bring the xAI Grok STT adapter in line with the other provider-specific cloud transcription adapters by allowing safe endpoint-query options while keeping API keys and provider-owned multipart fields out of user-editable query strings.

## Behavior

- xAI transcription endpoints may include safe query parameters such as `temperature=0.2` or `diarize=true`.
- Safe query parameters are copied into multipart request fields.
- Provider-owned fields are ignored from query options so users cannot override the file, model, language, or format fields that VoiceInk owns.
- The HTTP request URL strips query parameters before sending the xAI request.
- Secret-like query parameters such as `api_key` or `token` are rejected before any HTTP request.
- Existing xAI behavior remains unchanged for API-key lookup, language omission for `auto`, sanitized HTTP errors, and provider metadata.

## Open-Source Boundary

The feature uses only user-supplied endpoints and user-owned API keys. No commercial gates, telemetry, paid-account flows, or bundled secrets are added.
