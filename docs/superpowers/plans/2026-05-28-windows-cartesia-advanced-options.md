# Windows Cartesia Advanced Options Plan

## Slice

Map safe Cartesia endpoint query parameters into multipart STT fields and reject secret-bearing query parameters before HTTP.

## Tasks

1. Add failing Cartesia request-construction coverage for endpoint-query multipart options.
2. Add failing pre-HTTP coverage for secret-bearing endpoint query parameters.
3. Validate the configured endpoint before API-key use and HTTP requests.
4. Strip query parameters from the request URI and map safe options into multipart fields.
5. Update the completion tracker and verify focused infrastructure tests.

## Review Notes

- Do not change provider routing, API-key storage, response parsing, or error redaction.
- Do not let query options override `model`, `language`, or `file`.
- Keep user-owned credentials out of source, docs, tests, and logs.
