# Windows Cloud Provider Smoke Runbook Implementation Plan

## Task 1: Add Smoke Runbook

- Create a provider matrix for Custom, Groq, Deepgram, AssemblyAI, Mistral, Gemini, ElevenLabs, Soniox, Speechmatics, xAI, and Cartesia.
- Mark which providers support metadata probe, batch transcription, and live preview in the Windows fork.
- Include exact manual smoke steps for saving a key, testing the provider, recording a short phrase, checking History metadata, and clearing the key.

## Task 2: Update Tracker

- Update the completion bar to move live credential smoke documentation out of the near-term priority list.
- Keep actual credential smoke as a manual user-owned-key path, not an automated Codex task.

## Task 3: Verify And Commit

- Run `git diff --check`.
- Commit the docs-only slice.
