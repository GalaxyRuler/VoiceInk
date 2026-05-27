# Windows AssemblyAI Advanced Options Spec

## Intent

Let Windows users configure AssemblyAI transcript options through the existing endpoint field while keeping API keys and commercial account setup user-owned.

## Requirements

- Treat non-secret AssemblyAI endpoint query parameters as advanced transcript payload options.
- Map safe query parameters into the JSON transcript submission body.
- Preserve first-class payload fields for uploaded audio URL, model, language, and prompt.
- Parse boolean and numeric option values as JSON booleans/numbers instead of strings.
- Reject secret-bearing endpoint query parameters before any upload or transcript HTTP request.
- Keep upload, polling, provider routing, error redaction, and secure API-key storage unchanged.

## Open-Source Boundary

The feature is request-construction plumbing for user-owned AssemblyAI keys. It adds no bundled key, account flow, telemetry, licensing, paid gate, or upgrade surface.

## Verification

- Focused AssemblyAI cloud transcription request-construction tests.
- Focused pre-HTTP secret-query validation test.
