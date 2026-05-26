# Windows Speechmatics Live Preview Design

## Goal

Add Speechmatics realtime transcription as a best-effort live transcript preview source for the floating recorder, while keeping final transcription on the existing stopped-recording batch path.

## Grounding

- Speechmatics realtime docs describe a WebSocket API that starts with `StartRecognition`, accepts raw audio data, returns partial/final transcript messages, and ends with `EndOfStream`.
- The Windows recorder already streams copied 16 kHz mono PCM chunks to live preview providers and treats preview text as in-memory only.

## Requirements

- Start only when live transcript preview is enabled, the selected transcription provider is Speechmatics, and a Speechmatics API key is stored locally.
- Connect to the Speechmatics realtime WebSocket with `Authorization: Bearer <api key>`.
- Send a `StartRecognition` message configured for raw 16 kHz `pcm_s16le`, selected language, selected operating point, partials enabled, and low preview delay.
- Stream recorder audio chunks as binary WebSocket frames.
- Parse `AddPartialTranscript` and `AddTranscript` messages into preview text.
- Send `EndOfStream` on completion and close/abort without disrupting final transcription.

## Non-Goals

- Replacing the final Speechmatics batch transcription path.
- Persisting live preview text.
- Exposing advanced Speechmatics realtime options such as diarization or translation.
