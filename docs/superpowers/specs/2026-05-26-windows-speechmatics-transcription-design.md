# Windows Speechmatics Transcription Design

## Goal

Port the macOS Speechmatics cloud transcription option into the Windows fork as a free/open-source provider integration with secure local API-key storage and no commercial surfaces.

## Source Of Truth

- macOS provider: `VoiceInk\Transcription\Cloud\SpeechmaticsProvider.swift`
- Speechmatics batch quickstart: batch clients submit an audio file with a transcription config containing `transcription_config.language` and `operating_point`.
- Speechmatics authentication docs: Jobs API requests use `Authorization: Bearer <API key>` and regional endpoints such as `https://eu1.asr.api.speechmatics.com/v2/jobs/`.
- Speechmatics language docs: `language: "auto"` is supported for Batch transcription and `operating_point: "enhanced"` selects the higher-accuracy model.
- Speechmatics API reference: create jobs with `POST /jobs`, poll details with `GET /jobs/:jobid`, fetch text with `GET /jobs/:jobid/transcript`, and delete with `DELETE /jobs/:jobid`.

## Windows Behavior

- Add a `Speechmatics` cloud transcription preset.
- Use provider id `speechmatics`.
- Use model/default `speechmatics-enhanced`, matching the macOS provider's model name while translating it to Speechmatics `operating_point: "enhanced"` in the request payload.
- Default endpoint to `https://eu1.asr.api.speechmatics.com/v2/jobs`.
- Store credentials under `VoiceInk.Windows.Transcription.OpenAICompatible.Speechmatics.ApiKey`.
- Submit recorded audio as multipart form data:
  - `config`: JSON with `type: "transcription"` and `transcription_config`.
  - `data_file`: WAV stream.
- Preserve the app's current language setting:
  - `auto` is sent as `language: "auto"`.
  - Specific language codes are sent unchanged.
- Poll until the job is complete.
- Fetch the plain text transcript with `Accept: text/plain`.
- Attempt best-effort job deletion after completion or failure.
- Surface only sanitized provider errors, never API keys or response bodies.

## Deliberate Deferrals

- Realtime Speechmatics preview remains a later streaming slice.
- Provider test-call UI remains part of richer provider card parity.
- Advanced Speechmatics features such as diarization, summaries, and expected language lists remain future settings work.
