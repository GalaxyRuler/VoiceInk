# Windows Speechmatics Advanced Options Spec

## Intent

Let Windows users configure Speechmatics batch transcription options through the existing endpoint field while keeping the jobs URL stable for create, poll, transcript, and cleanup requests.

## Requirements

- Treat non-secret Speechmatics endpoint query parameters as advanced `transcription_config` options.
- Strip endpoint query parameters from the jobs URI before HTTP requests and job path composition.
- Map safe scalar query values into `transcription_config` with JSON boolean/number parsing.
- Preserve first-class language and operating-point settings.
- Reject secret-bearing endpoint query parameters before any Speechmatics HTTP request.
- Keep multipart upload, polling, transcript fetch, cleanup, API-key storage, and error redaction unchanged.

## Open-Source Boundary

The feature is request-construction plumbing for user-owned Speechmatics keys. It adds no bundled credential, account flow, telemetry, licensing, paid gate, or commercial updater.

## Verification

- Focused Speechmatics cloud transcription request-construction tests.
- Focused pre-HTTP secret-query validation test.
