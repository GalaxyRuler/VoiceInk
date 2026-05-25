# Windows xAI Transcription Plan

## Scope

Add xAI Grok batch transcription to the existing Windows cloud provider architecture.

## Steps

1. Add red catalog and secret-name tests for the xAI transcription preset.
2. Add red router coverage proving `xai` routes to an xAI-specific adapter.
3. Add red adapter tests for multipart request construction, bearer auth, language handling, missing key, empty text, and sanitized errors.
4. Implement the catalog, secret mapping, adapter, router, and app wiring.
5. Update the completion tracker.
6. Run focused tests, full tests, build, diff hygiene, and commit the slice.
