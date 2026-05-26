# Windows Cartesia Transcription Plan

## Scope

Add Cartesia Ink Whisper batch transcription to the Windows cloud provider architecture.

## Steps

1. Add red catalog and secret-name tests for the Cartesia transcription preset.
2. Add red router coverage proving `cartesia` routes to a Cartesia-specific adapter.
3. Add red adapter tests for multipart request construction, bearer auth, Cartesia version header, language handling, missing key, empty text, and sanitized errors.
4. Implement the catalog, secret mapping, adapter, router, and app wiring.
5. Update the completion tracker.
6. Run focused tests, full tests, build, diff hygiene, and commit the slice.
