# Windows AssemblyAI Advanced Options Plan

## Slice

Map safe AssemblyAI endpoint query parameters into transcript submission JSON so advanced options such as speaker labels, formatting, and expected speaker count can be configured without new settings storage.

## Tasks

1. Add a failing AssemblyAI request-construction test for endpoint-query options.
2. Add a failing pre-HTTP guard for secret-bearing endpoint query parameters.
3. Validate the configured endpoint before upload.
4. Map safe query options into the transcript payload with JSON boolean/number parsing.
5. Update the completion tracker and verify focused infrastructure tests.

## Review Notes

- Do not change upload, polling, provider routing, or API-key storage.
- Do not allow query options to override `audio_url`, model, language, or prompt.
- Keep user-owned credentials out of source, docs, tests, and logs.
