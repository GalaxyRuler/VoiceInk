# Windows ElevenLabs Advanced Options Plan

## Slice

Map safe ElevenLabs endpoint query parameters into multipart speech-to-text fields so advanced options such as diarization can be configured through the existing endpoint field.

## Tasks

1. Add a failing ElevenLabs request-construction test for endpoint-query advanced options.
2. Strip safe endpoint query parameters from the request URI and map them into multipart fields.
3. Preserve model/language/file as first-class fields and ignore duplicate reserved query names.
4. Update the completion tracker and verify the focused infrastructure tests.

## Review Notes

- Do not add dependencies or new settings persistence.
- Do not alter API-key storage, provider routing, or error body redaction.
- Keep query-secret rejection in `TranscriptionConfiguration` as the safety boundary.
