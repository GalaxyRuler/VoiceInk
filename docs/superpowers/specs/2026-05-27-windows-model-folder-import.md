# Windows Model Folder Import

## Goal

Let users import a folder of local whisper.cpp GGML `.bin` models instead of adding each model file one by one.

## Source Of Truth

- VoiceInk’s local transcription path uses whisper.cpp GGML model files.
- The Windows model library already supports individual `.bin` imports, catalog downloads, model health checks, and stale-reference cleanup.

## Online Grounding

- whisper.cpp documents model filenames such as `ggml-base.en.bin`.
- whisper.cpp model guidance describes GGML `.bin` files as the local model format.
- Reference: https://github.com/ggml-org/whisper.cpp/blob/master/models/README.md

## Requirements

- The Models page must expose an `Import Folder` action near the existing model import controls.
- Folder import must scan `.bin` files in the selected folder and add usable references to imported models.
- Duplicate paths must be skipped case-insensitively.
- Non-`.bin` paths must be ignored by the merge logic.
- Folder import must not delete, move, or copy user model files.
- Empty folders or folders without new `.bin` files must produce a clear status.

## Non-Goals

- No recursive disk-wide search.
- No model conversion.
- No checksum download verification in this slice.
