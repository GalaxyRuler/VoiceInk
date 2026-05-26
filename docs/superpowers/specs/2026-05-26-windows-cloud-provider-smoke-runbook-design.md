# Windows Cloud Provider Smoke Runbook Design

## Goal

Document a safe, repeatable manual smoke path for cloud transcription providers and live preview providers without requiring Codex to possess API keys, paid accounts, or secrets.

## Grounding

- Deepgram live transcription uses the `wss://api.deepgram.com/v1/listen` live endpoint.
- AssemblyAI Universal Streaming uses `wss://streaming.assemblyai.com/v3/ws`.
- Speechmatics documents realtime WebSocket and batch Jobs API flows.
- Cartesia documents Ink/Ink-Whisper Speech-to-Text for batch and streaming-oriented use.
- The Windows app stores provider keys in Windows Credential Manager and excludes keys from JSON settings backup.

## Scope

- Add a runbook under `docs/superpowers/` with provider setup, metadata probe, batch recording, live preview, history, and cleanup checks.
- Include clear secret-handling rules.
- Do not add real API keys, `.env` files, recordings, transcripts, or provider responses to the repo.
- Keep the runbook provider-neutral and open-source friendly.

## Verification

- Docs-only slice: run `git diff --check` and confirm no secret-like placeholders are introduced.
