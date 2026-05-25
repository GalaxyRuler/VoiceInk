# Windows AssemblyAI Live Preview Design

Date: 2026-05-26

## Purpose

Expand Windows floating-recorder live transcript preview beyond Deepgram by adding AssemblyAI streaming support and routing live preview startup through provider-aware services.

## Source Of Truth

The macOS app includes `AssemblyAIProvider` and `AssemblyAIStreamingProvider` as first-class transcription providers. AssemblyAI models include `universal-3-pro` and `universal-streaming`, both marked as streaming-capable. The Windows app already has the live preview UI and Deepgram streaming plumbing; this slice should reuse that recorder/UI path and add AssemblyAI without changing the final transcription path.

## External Grounding

AssemblyAI documents its streaming endpoint as `wss://streaming.assemblyai.com/v3/ws`, with `speech_model=u3-rt-pro` for Universal-3 Pro streaming, `sample_rate=16000`, PCM S16LE audio, `Turn` transcript events with `end_of_turn`, and a `Terminate` message for ending streaming sessions. No API key is available in this environment, so verification is limited to request construction, parser behavior, socket/session flow with fakes, tests, and build.

## Behavior

- Add an AssemblyAI transcription preset with:
  - id `assemblyai`;
  - display name `AssemblyAI`;
  - endpoint `https://streaming.assemblyai.com/v3/ws`;
  - default model `universal-3-pro`;
  - models `universal-3-pro` and `universal-streaming`.
- When live preview is enabled and the selected cloud transcription provider is AssemblyAI:
  - read the provider-specific API key from the existing transcription secret naming scheme;
  - connect to the AssemblyAI streaming WebSocket;
  - send PCM16 audio chunks as binary frames;
  - send an AssemblyAI `Terminate` text message before closing a completed stream;
  - emit interim/partial transcript text to the recorder preview;
  - combine committed/final text with the current partial when message metadata indicates a final turn.
- When the provider is not supported by a live preview service or the API key is missing, return `null` so final transcription still proceeds.
- Keep failures best-effort: live preview errors must not disrupt recording or final transcription.

## Non-Goals

- Do not add paid account flows, provider signup prompts, telemetry, trial UI, or commercial gates.
- Do not manually test with a real AssemblyAI key in this environment.
- Batch AssemblyAI transcription is included so the selectable provider does not fall back to the OpenAI-compatible multipart path after recording.
- Do not add dictionary/prompt streaming request fields unless the public request format is implemented and testable.

## Verification

- Failing tests first for AssemblyAI URI construction, parser behavior, live session flow, batch request construction/polling, provider routing, and preset catalog.
- Focused tests for the new transcription preview classes.
- Full solution tests and Debug x64 build.
- Manual smoke remains limited to no-secret behavior unless a tester supplies an API key.
