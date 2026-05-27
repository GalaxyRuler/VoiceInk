# Windows Local Model Filename Guidance

## Goal

Make the local model library overview explicitly tell users what whisper.cpp GGML model filenames usually look like before they import a model from disk.

## Source Of Truth

The Windows app uses Whisper.net over whisper.cpp GGML model files. The upstream whisper.cpp model documentation commonly references files such as `ggml-base.en.bin`, and the Windows UI already uses that filename pattern in model health repair rows and placeholders. The model library overview should include the same expectation near download/import/storage guidance.

## Requirements

- The model library overview must include a storage guidance row titled `Expected Filename`.
- The row value must be `ggml-*.bin`.
- The row detail must mention `ggml-base.en.bin`.
- The guidance must be informational only and must not reject custom `.bin` files by filename alone.
- The row must be presenter-backed so the existing WinUI list renders it.

## Non-Goals

- This slice does not change model validation rules.
- This slice does not download, move, or delete model files.
