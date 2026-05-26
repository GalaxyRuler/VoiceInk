# Windows Deepgram Query Options Design

## Goal

Let users configure advanced Deepgram transcription options through the endpoint query string without VoiceInk adding conflicting duplicate defaults.

## Grounding

- Deepgram documents optional query parameters including `smart_format`, `paragraphs`, `utterances`, `diarize`, and `diarize_model`.
- Deepgram recommends `diarize_model=latest` for batch diarization where supported.
- VoiceInk already exposes an editable provider endpoint field, so advanced options can stay local and user-owned without new account, telemetry, or paid feature surfaces.

## Behavior

- Preserve existing endpoint query parameters for Deepgram batch transcription.
- Continue adding `model` from VoiceInk's model field.
- Add default `smart_format=true` only when the endpoint query does not already contain `smart_format`.
- Add `language` from VoiceInk settings only when the selected language is not automatic and the endpoint query does not already contain `language`.
- Do not introduce bundled keys, paid gates, or provider account flows.

## Verification

- Add focused Deepgram request URI tests.
- Run focused Infrastructure Deepgram tests, full solution tests, Debug x64 build, and `git diff --check`.
