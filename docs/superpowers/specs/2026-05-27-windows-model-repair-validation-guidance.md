# Windows Model Repair Validation Guidance Spec

Date: 2026-05-27

## Context

VoiceInk for Windows already supports local Whisper model catalog cards, downloads, imported `.bin` references, warmup, repair actions, and health rows for ready and broken models. The remaining parity gap for this slice is repair clarity: users should understand that repairing a broken local model reference validates a file they explicitly choose, rather than scanning folders or mutating model files.

The whisper.cpp project documents Whisper model files in a custom GGML format, commonly named `ggml-*.bin`. Windows app file-picker guidance favors user-selected files for granting app access to local files. VoiceInk should surface this boundary in the repair guidance.

## Requirements

- Broken local model health guidance must include a `Repair Picker` row.
- The row must say repair validates the `.bin` file the user chooses and does not scan folders automatically.
- The row must appear only in repair guidance, not ready model guidance.
- The row must be core presenter output with unit-test coverage.

## Non-Goals

- Do not change model validation thresholds.
- Do not add folder scanning.
- Do not change model import behavior.
- Do not add a dependency.

## Acceptance Criteria

- `LocalWhisperModelHealthPresenter.Present` includes `Repair Picker` in repair guidance rows.
- Focused model health presenter tests cover the row title, value, detail, and status badge.
- Project completion documentation reflects the model repair validation slice.
- Focused tests, full solution tests, Debug x64 build, and whitespace validation pass.
