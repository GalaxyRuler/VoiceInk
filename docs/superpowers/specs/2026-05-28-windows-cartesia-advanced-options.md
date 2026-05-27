# Windows Cartesia Advanced Options Spec

## Intent

Let Windows users configure Cartesia batch speech-to-text options through the existing endpoint field while preserving the official multipart request shape.

## Requirements

- Treat non-secret Cartesia endpoint query parameters as advanced multipart form fields.
- Strip endpoint query parameters from the request URI after mapping them into form fields.
- Preserve first-class model, language, and file fields.
- Reject secret-bearing endpoint query parameters before any HTTP request.
- Keep Cartesia version header, API-key storage, provider routing, response parsing, and sanitized errors unchanged.

## Open-Source Boundary

The feature is request-construction plumbing for user-owned Cartesia keys. It adds no bundled credential, account flow, telemetry, licensing, paid gate, or commercial updater.

## Verification

- Focused Cartesia cloud transcription request-construction tests.
- Focused pre-HTTP secret-query validation test.
