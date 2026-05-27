# Windows Model Compatibility Guidance

## Goal

Make the Windows Model Library overview explain that VoiceInk checks imported `.bin` files for whisper.cpp GGML compatibility before warmup or transcription.

## Source Of Truth

- whisper.cpp distributes local Whisper models as GGML `.bin` files, commonly named `ggml-*.bin`.
- The Windows implementation already validates selected model paths, extension, size, and optional GGML header bytes.
- The Model Library overview should make repair expectations visible before users hit a failed warmup or dictation attempt.

## Requirements

- Add a Model Library storage guidance row for compatibility checks.
- Mention GGML header validation and that invalid `.bin` files are rejected before warmup.
- Keep the row presenter-backed and covered by Core tests.
- Do not change model validation behavior in this slice.

## Non-Goals

- No new model download source.
- No recursive folder scan.
- No automatic repair or deletion.
