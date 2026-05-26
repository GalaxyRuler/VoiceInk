# Windows Speechmatics And Cartesia Provider Probes Design

Cloud transcription provider tests should verify saved API keys with small metadata/auth requests and must not upload audio, start transcription jobs, or expose provider response bodies.

Grounding:

- Speechmatics Batch SaaS docs use `Authorization: Bearer <token>` and the `https://...asr.api.speechmatics.com/v2/jobs/` API for job listing/status.
- Cartesia API conventions use `https://api.cartesia.ai`, `Authorization: Bearer <api_key>`, and a required `Cartesia-Version` header. Cartesia metadata endpoints such as datasets list are safe for key validation without STT usage.

## Requirements

- Speechmatics provider test sends `GET https://eu1.asr.api.speechmatics.com/v2/jobs?limit=1` with Bearer auth.
- Cartesia provider test sends `GET https://api.cartesia.ai/datasets/?limit=1` with Bearer auth and `Cartesia-Version: 2026-03-01`.
- Missing keys short-circuit before HTTP.
- Rejected responses return sanitized status-only messages.
- No audio upload, transcription job creation, or response-body disclosure.

## Non-Goals

- Live provider smoke using real user keys.
- Cartesia admin API key metadata checks.
- Speechmatics job creation or transcript retrieval.
