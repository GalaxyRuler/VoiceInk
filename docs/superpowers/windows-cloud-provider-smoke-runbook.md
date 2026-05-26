# Windows Cloud Provider Smoke Runbook

Last updated: 2026-05-26

This runbook verifies cloud transcription behavior with user-owned provider keys. Do not write keys, account IDs, raw provider responses, `.env` files, recordings, or transcripts into the repo.

## Safety Rules

- Use only throwaway provider keys created by the maintainer.
- Save keys through the app UI so they go to Windows Credential Manager.
- Do not paste keys into logs, docs, Git commits, screenshots, or issue text.
- After each provider smoke, clear the key from the app UI unless the maintainer intentionally wants it retained locally.
- Use a short benign phrase such as `VoiceInk smoke test one two three`.

## Common Setup

1. Launch the Windows app from the short path when needed:
   `W:\.dotnet-sdk-10\dotnet.exe run --project VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64`
2. Open `AI Models`.
3. Select `OpenAI-compatible` as the transcription provider.
4. Pick the provider preset.
5. Confirm endpoint and model are filled by the preset, or enter the provider-specific endpoint/model when using `Custom`.
6. Paste the provider key into the key box and click `Save Key`.
7. Click `Test Provider`.
8. Record a short phrase from Dashboard.
9. Confirm History includes provider/model metadata and a completed row.
10. If live preview is expected, enable `Show Live Transcript Preview`, record again, and confirm partial text appears only during recording.
11. Clear the provider key.

## Provider Matrix

| Provider | Metadata Probe | Batch Recording | Live Preview | Notes |
| --- | --- | --- | --- | --- |
| Custom OpenAI-compatible | Endpoint-dependent | Yes | No | Use an HTTPS OpenAI-style audio transcription endpoint. Localhost HTTP is accepted only for development. |
| Groq | Yes | Yes | No | Uses OpenAI-compatible multipart transcription. |
| Deepgram | Yes | Yes | Yes | Live preview uses Deepgram listen WebSocket. |
| AssemblyAI | Yes | Yes | Yes | Batch uses upload/transcript polling; live preview uses Universal Streaming. |
| Mistral | Yes | Yes | No | Uses OpenAI-compatible Voxtral transcription path. |
| Gemini | Yes | Yes | No | Supports inline audio and Files API fallback in the Windows adapter. |
| ElevenLabs | Yes | Yes | No | Uses Scribe batch transcription. |
| Soniox | Yes | Yes | Yes | Batch uses async upload/transcription polling; live preview uses realtime tokens. |
| Speechmatics | Yes | Yes | Yes | Batch uses Jobs API; live preview uses realtime WebSocket. |
| xAI | Yes | Yes | No | Uses Grok STT batch transcription. |
| Cartesia | Yes | Yes | Yes | Batch uses Ink Whisper; live preview uses Cartesia streaming parser path. |

## Expected Results

- `Save Key` reports the selected provider key was saved.
- `Test Provider` reports a provider-specific success or clear provider error without uploading audio.
- A successful recording inserts text and writes a completed History row.
- History detail shows provider/model metadata matching the selected preset.
- Live preview providers show transient partial text during recording and clear it at recording boundaries.
- `Export Settings` does not include provider keys.
- `Clear Key` removes the provider key from Windows Credential Manager for that preset.

## Failure Notes

- `401` or `403`: clear and re-save the key, then verify the provider account has access to the selected model.
- `404` or model-not-found: select a model from the provider card or enter a currently available model name.
- WebSocket close during live preview: keep the completed batch result as the source of truth, then inspect provider access/model support.
- Microphone unavailable: open `Permissions` or `Audio Input`, refresh devices, and confirm Windows microphone privacy settings.
