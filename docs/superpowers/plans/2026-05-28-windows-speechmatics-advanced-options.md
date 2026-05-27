# Windows Speechmatics Advanced Options Plan

## Slice

Map safe Speechmatics endpoint query parameters into batch `transcription_config` fields and keep the jobs URI query-free for create/poll/transcript/delete paths.

## Tasks

1. Add failing Speechmatics request-construction coverage for endpoint-query config options.
2. Add failing pre-HTTP coverage for secret-bearing endpoint query parameters.
3. Validate the configured endpoint before API-key use and HTTP requests.
4. Strip endpoint query parameters from the jobs URI and map safe scalar options into config JSON.
5. Update the completion tracker and verify focused infrastructure tests.

## Review Notes

- Do not change upload, polling, transcript fetch, cleanup, provider routing, or API-key storage.
- Do not let query options override the first-class language or operating-point choices.
- Keep user-owned credentials out of source, docs, tests, and logs.
