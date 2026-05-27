# Windows VAD Threshold Parity

## Goal

Align the Windows local WAV voice activity detector's default minimum speech duration with the macOS VoiceInk whisper.cpp VAD settings.

## Source Of Truth

- `VoiceInk/Transcription/Whisper/LibWhisper.swift` sets `vadParams.min_speech_duration_ms = 250`.
- `VoiceInk/AppDefaults.swift` keeps VAD enabled by default.
- `VoiceInk/Views/ModelSettingsView.swift` exposes the user-facing VAD toggle.
- Upstream whisper.cpp exposes a `vad_min_speech_duration_ms` parameter for VAD filtering.

## Requirements

- Change the Windows default minimum voiced duration from 150 ms to 250 ms.
- Keep the explicit constructor override seam for tests and future tuning.
- Add boundary coverage:
  - 200 ms of voiced PCM is not speech by default.
  - 250 ms of voiced PCM is speech by default.
  - an explicit shorter threshold can still classify a shorter burst as speech.
- Preserve fail-open behavior for unreadable or unsupported audio.

## Non-Goals

- Do not add a user-facing threshold setting in this slice.
- Do not replace the lightweight WAV detector with a full whisper.cpp VAD model in this slice.
- Do not alter cloud-provider VAD behavior.
