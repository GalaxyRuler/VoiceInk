# Windows Voice Activity Detection Spec

## Goal

Add a conservative Windows voice activity detection path that matches the macOS app's default-on VAD setting while avoiding destructive false negatives.

## Source Of Truth

- macOS default: `VoiceInk/AppDefaults.swift`
- macOS settings toggle: `VoiceInk/Views/ModelSettingsView.swift`
- macOS transcription/VAD integration: `VoiceInk/Transcription/Whisper/LibWhisper.swift`
- Windows recording pipeline: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`

## Requirements

- Add `IsVadEnabled`, defaulting to `true`, to Windows settings.
- Expose a Windows settings toggle named Voice Activity Detection (VAD).
- Keep VAD behind an injectable Core interface for testability.
- When VAD is enabled and the detector confidently finds no speech, skip transcription/insertion/history and report `No speech detected`.
- When VAD is disabled, do not analyze audio.
- Native detector must fail open for unreadable or unsupported files so recordings are not discarded by mistake.
- Use only local audio analysis; no paid services, accounts, telemetry, or cloud calls.

## Verification

- Dictation controller tests for enabled/no-speech and disabled/bypass behavior.
- Native WAV detector tests for silence, speech, and fail-open behavior.
- Settings persistence tests for the default and saved toggle.
- App project build, full solution tests, and Debug x64 build before commit.
