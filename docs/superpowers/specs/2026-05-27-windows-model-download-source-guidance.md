# Windows Model Download Source Guidance Spec

Date: 2026-05-27

## Goal

Make the local model library more transparent by showing that catalog downloads use open-source whisper.cpp GGML model files hosted on Hugging Face.

## Source Of Truth

The Windows app uses Whisper.net over whisper.cpp-compatible GGML `.bin` files. The upstream whisper.cpp model documentation and Hugging Face repository are the open-source model source for the catalog downloads.

## Windows Behavior

- Add a presenter-backed `Download Source` storage guidance row to the AI Models overview.
- The row names `whisper.cpp GGML` and explains that catalog downloads come from open-source model files hosted on Hugging Face.
- Keep model download URLs, import behavior, storage locations, validation, warmup, and default selection unchanged.

## Non-Goals

- Do not change the model catalog or download endpoints.
- Do not add account-gated model providers, telemetry, or commercial model prompts.
- Do not download or validate models during this UI guidance slice.
