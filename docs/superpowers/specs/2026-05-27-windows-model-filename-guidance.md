# Windows Model Filename Guidance Design

## Context

VoiceInk for Windows uses whisper.cpp-compatible local model files. The model health presenter already validates `.bin` files and explains repair/warmup states, but it did not show the common whisper.cpp `ggml-*.bin` filename convention in the persistent health rows.

Whisper.cpp model documentation and common model repositories use names such as `ggml-base.en.bin`. The Windows fork should make that expectation visible during both ready and repair states so imported model paths are easier to diagnose.

## Goal

Add a model health guidance row for the expected filename convention:

- ready models show `Filename` / `ggml-*.bin` with an example `ggml-base.en.bin`;
- broken or missing models show the same convention as repair guidance;
- the row is emitted by `LocalWhisperModelHealthPresenter` and remains UI-independent.

## Non-Goals

- No stricter filename validation. Custom `.bin` paths remain allowed.
- No model file rename or movement.
- No network download changes.
- No dependency changes.

## Testability

Focused Core presenter tests cover the filename convention row for both ready and repair model states.
