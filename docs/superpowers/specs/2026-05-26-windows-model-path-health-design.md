# Windows Model Path Health Design

## Goal

Make local Whisper model selection safer by surfacing invalid or unavailable model paths before recording or warmup fails.

## Grounding

- The Windows fork uses whisper.cpp GGML `.bin` files through Whisper.net.
- The official whisper.cpp model distribution publishes `.bin` files such as `ggml-base.en.bin` from Hugging Face.
- Local models can be moved, deleted, partially downloaded, or replaced by non-model files after they are selected.

## Behavior

- Add a Core model path health check that classifies the selected local model path as no selection, invalid extension, missing file, empty file, suspiciously small file, or ready.
- Keep file-system access behind delegates so Core remains UI-independent and testable.
- Show the health message in the AI Models section next to the default model status.
- Disable manual warmup, Set as Default, and Show in Explorer for unavailable or invalid local model paths.
- Preserve existing import/download behavior and avoid deleting or moving any user model files.

## Verification

- Add focused Core tests for every model-path health state.
- Run focused model tests, full solution tests, Debug x64 build, and `git diff --check`.
