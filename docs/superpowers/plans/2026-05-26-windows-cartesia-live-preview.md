# Windows Cartesia Live Preview Plan

## Scope

Add Cartesia realtime preview support to the existing composite live transcription preview pipeline.

## Steps

1. Add red tests for Cartesia streaming URI construction and message parsing.
2. Add red live preview service tests proving header-based connection, audio sending, transcript emission, and finalize/close commands.
3. Extend the WebSocket abstraction to support provider-specific headers while preserving existing Authorization-based providers.
4. Implement Cartesia URI factory, parser, preview service, and app wiring.
5. Update the completion tracker.
6. Run focused tests, full tests, build, diff hygiene, and commit the slice.
