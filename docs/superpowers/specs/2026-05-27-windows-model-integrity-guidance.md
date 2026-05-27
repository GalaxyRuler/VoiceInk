# Windows Model Integrity Guidance

## Goal

Improve local model-management parity by making model provenance and integrity expectations explicit for downloaded, imported, and manually copied whisper.cpp GGML model files.

## Source Of Truth

- Upstream whisper.cpp documents GGML `.bin` model files such as `ggml-base.en.bin` and public model downloads from Hugging Face.
- VoiceInk Windows already validates `.bin` extension and GGML header compatibility before warmup/use.
- Users may import or manually download model binaries outside VoiceInk's catalog flow, so the UI should tell them to compare hashes when a source publishes checksums.

## Requirements

- The Local Whisper Library storage guidance includes an Integrity Check row.
- The row tells users to compare imported/manually downloaded model hashes with source release checksums when checksums are published.
- The row must not imply VoiceInk downloads private models, phones home, or automatically trusts arbitrary files beyond local GGML compatibility checks.

## Acceptance

- Focused tests fail before implementation because the integrity guidance row is absent.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
