# Windows Soniox Advanced Options Plan

## Slice

Map safe Soniox endpoint query parameters into async create-transcription payload fields and fail secret-bearing endpoint queries before upload.

## Tasks

1. Add failing Soniox request-construction coverage for endpoint-query payload options.
2. Add failing pre-upload coverage for secret-bearing endpoint query parameters.
3. Validate the configured endpoint before API-key use and file upload.
4. Map safe scalar options into the Soniox create payload with JSON boolean/number parsing.
5. Update the completion tracker and verify focused infrastructure tests.

## Review Notes

- Do not change upload, polling, transcript fetch, cleanup, provider routing, or API-key storage.
- Do not let query options override `model`, `file_id`, or `language_hints`.
- Keep user-owned credentials out of source, docs, tests, and logs.
