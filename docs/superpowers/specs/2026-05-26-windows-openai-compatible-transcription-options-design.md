# Windows OpenAI-Compatible Transcription Options Design

## Goal

Let users request OpenAI-compatible transcription response formats such as `verbose_json` while preserving VoiceInk's existing text extraction and safe local configuration model.

## Grounding

- OpenAI's audio transcription API documents `response_format` values including `json`, `text`, `srt`, `verbose_json`, `vtt`, and `diarized_json`.
- OpenAI documents that timestamp granularities require `response_format=verbose_json`.
- VoiceInk already exposes a provider endpoint field, so response-format selection can remain a local user-owned option without adding any commercial surface.

## Behavior

- Keep default OpenAI-compatible multipart `response_format=json`.
- When the configured endpoint query includes `response_format`, use that value for the multipart `response_format` field.
- Preserve endpoint validation that rejects credential-bearing query parameters.
- Continue parsing `text` from JSON-compatible responses.

## Verification

- Add focused OpenAI-compatible transcription request tests.
- Run focused Infrastructure tests, full solution tests, Debug x64 build, and `git diff --check`.
