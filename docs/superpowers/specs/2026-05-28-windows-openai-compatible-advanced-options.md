# Windows OpenAI-Compatible Advanced Transcription Options

## Goal

Make the Custom, Groq, Mistral, and other OpenAI-compatible transcription path flexible enough for provider-specific multipart options while keeping VoiceInk-owned fields and secrets protected.

## Behavior

- Safe endpoint-query parameters are copied into multipart form fields.
- `response_format` continues to be handled as the explicit response-format field, defaulting to `json`.
- VoiceInk-owned multipart fields are ignored from generic query promotion:
  - `file`
  - `language`
  - `model`
  - `prompt`
  - `response_format`
- The outgoing URI keeps its query string so existing custom endpoints and local proxies that rely on URL-level query options continue to work.
- Secret-like query parameters are still rejected by shared cloud endpoint validation before HTTP.
- Existing secure key lookup, provider-specific secret names, language and prompt handling, sanitized errors, and loopback development behavior remain unchanged.

## Open-Source Boundary

The feature only supports user-controlled provider endpoints and user-owned API keys. It adds no bundled credentials, paywall, telemetry, account flow, or commercial provider gate.
