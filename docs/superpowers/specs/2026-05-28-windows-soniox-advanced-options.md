# Windows Soniox Advanced Options Spec

## Intent

Let Windows users configure Soniox async transcription options through the existing endpoint field while keeping upload, polling, and cleanup behavior unchanged.

## Requirements

- Treat non-secret Soniox endpoint query parameters as advanced create-transcription payload options.
- Map safe query parameters into the JSON create payload with boolean/number parsing.
- Preserve first-class model, file ID, and language hint settings.
- Reject secret-bearing endpoint query parameters before file upload.
- Keep Soniox API-key storage, upload, polling, transcript fetch, cleanup, and sanitized errors unchanged.

## Open-Source Boundary

The feature is request-construction plumbing for user-owned Soniox keys. It adds no bundled credential, account flow, telemetry, licensing, paid gate, or commercial updater.

## Verification

- Focused Soniox cloud transcription request-construction tests.
- Focused pre-upload secret-query validation test.
