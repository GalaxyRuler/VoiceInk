# Windows Deepgram Live Preview Design

## Goal

Add the first real Windows streaming partial transcript source for the floating recorder live transcript preview, using Deepgram streaming interim results. This should mirror the macOS architecture: the recorder emits raw PCM chunks while still saving the complete recording, a streaming provider turns those chunks into partial text, and the recorder preview displays only provider-supplied text.

## Source Of Truth

macOS references inspected:

- `VoiceInk/CoreAudioRecorder.swift`: records 16 kHz mono PCM Int16 for transcription and forwards the same PCM bytes through `onAudioChunk` for streaming.
- `VoiceInk/Recorder.swift`: exposes `onAudioChunk` and forwards it into the active recorder.
- `VoiceInk/Transcription/Streaming/StreamingTranscriptionProvider.swift`: defines session start, audio chunk send, commit, disconnect, and partial/committed events.
- `VoiceInk/Transcription/Streaming/StreamingTranscriptionService.swift`: consumes audio chunks, forwards provider partials to live preview, accumulates committed text, and cleans up on cancel/stop.
- `VoiceInk/Transcription/Streaming/DeepgramStreamingProvider.swift`: connects only when a Deepgram key exists and maps provider partial/committed events into VoiceInk streaming events.
- `VoiceInk/Transcription/Cloud/DeepgramProvider.swift`: defines Deepgram `nova-3` models with streaming support.

External documentation checked:

- Deepgram Interim Results docs: live streaming with `interim_results=true`, raw `linear16`, `channels=1`, and `sample_rate=16000` returns preliminary transcripts during streaming.
- Deepgram Encoding/Sample Rate docs: raw audio streams must specify `encoding` and `sample_rate`.
- Microsoft `ClientWebSocket.SendAsync` docs: exactly one send and one receive operation is supported in parallel on each websocket, so audio sends must be serialized.

## Windows Scope

Implemented in this slice:

- Add Core audio chunk publishing contracts for 16 kHz mono PCM Int16 chunks.
- Extend `NAudioCaptureService` to publish the same PCM chunks it writes to the WAV file.
- Add Core live transcription preview contracts and integrate them into `DictationController`.
- Add Deepgram as a Windows cloud transcription preset with `nova-3` and `nova-3-medical`.
- Add a direct Deepgram batch transcription service so selecting Deepgram can still produce the final stopped transcript.
- Add a Deepgram live preview service that:
  - starts only for the Deepgram preset when `Show Live Transcript Preview` is enabled,
  - reads the provider-specific API key from Windows Credential Manager through the existing secret abstraction,
  - connects to the Deepgram websocket with `interim_results=true`, `encoding=linear16`, `channels=1`, and `sample_rate=16000`,
  - serializes audio sends through a queue,
  - parses Deepgram interim/final transcript messages,
  - updates `DictationController.UpdatePartialTranscript(...)` only from real provider text,
  - shuts down on stop, cancel, or controller failure.
- Keep live preview best-effort. Preview failures should not break local recording or final transcription.
- Bound live-preview connection and cleanup waits. A slow or stuck websocket connect/send/close must time out, abort/dispose the preview socket, and let recording start/stop/cancel continue.

Not implemented in this slice:

- Streaming final text as the primary transcription result. The stopped recording still runs through the selected final transcription service.
- AssemblyAI, Speechmatics, Soniox, Cartesia, xAI, ElevenLabs, Mistral, or local streaming providers.
- Fake local partials from audio levels or final transcript text.

## Data Flow

`NAudioCaptureService` raises `AudioChunkAvailable` with copied PCM bytes after each `WaveInEvent.DataAvailable`. `DictationController` starts a live preview session after recording starts when settings and provider support it. During recording, audio chunk events are enqueued into the session. The Deepgram session sends chunks over a websocket, receives transcript messages, and calls back with a running partial text. The controller accepts those updates only while recording and clears them before stop/cancel continues.

When the user stops recording, the controller unsubscribes from audio chunks, closes the live preview session, clears partial text, then proceeds with the normal final transcription pipeline. The final provider result remains the source of saved history, metrics, enhancement, and inserted text.

## Safety And Privacy

Deepgram streaming sends microphone audio to Deepgram only when the user selects the Deepgram provider, has configured its API key locally, and enables live transcript preview. API keys stay in Windows Credential Manager and are not written to JSON settings or backups. Live preview text stays in memory and is not written to logs, diagnostics, history, settings, metrics, or backups.

If the streaming preview connection fails, recording continues and final transcription still uses the selected provider. Errors are sanitized and do not include API keys or response bodies.

Preview sockets are aborted and disposed on failed connect or bounded cleanup timeout so repeated network failures do not leak websocket resources or block stop/cancel.

## Verification

- Add Core tests for controller live preview session lifecycle and partial update gating.
- Add Infrastructure tests for Deepgram preset/secret mapping, batch request construction, response parsing, sanitized errors, streaming URI construction, and streaming message parsing.
- Add Native/app build verification for NAudio event and DI wiring.
