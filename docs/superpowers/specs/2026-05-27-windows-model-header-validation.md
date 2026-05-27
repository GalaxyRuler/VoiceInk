# Windows Model Header Validation

## Goal

Improve local Whisper model lifecycle parity by detecting obviously invalid `.bin` files before warmup or recording, instead of accepting any large file with a `.bin` extension.

## Source Of Truth

- The macOS app treats local models as whisper.cpp GGML files.
- whisper.cpp model files conventionally use `ggml-*.bin` names.
- whisper.cpp GGML model files include the GGML magic header `0x67676d6c`.

## Requirements

- Keep extension, existence, empty, and suspicious-size checks unchanged.
- When a caller provides model-header bytes, classify non-GGML headers as unusable.
- Add user-facing repair guidance for invalid model headers.
- Keep the header check optional so existing tests and callers that only know file length continue to work.
- Do not read arbitrary folders or delete files.

## Non-Goals

- No full model parsing.
- No checksum catalog.
- No network fetch.
- No changes to warmup implementation.
