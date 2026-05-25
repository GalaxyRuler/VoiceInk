# Windows Live Transcript Preview Design

## Goal

Add the Windows-side recorder and settings plumbing for macOS-style live transcript preview without fabricating transcript text. The recorder should show partial transcript text only when a real partial transcript source has provided text and the user has enabled the preview setting.

## Source Of Truth

macOS references inspected:

- `VoiceInk/Views/Recorder/MiniRecorderView.swift`: expands the mini recorder only when `showLiveTextPreview` is enabled, the state is recording, and `partialTranscript` is non-empty.
- `VoiceInk/Views/Recorder/RecorderComponents.swift`: `LiveTranscriptView` uses a compact scrolling text panel above the recorder controls.
- `VoiceInk/Transcription/Engine/VoiceInkEngine.swift`: resets `partialTranscript` at recording boundaries and updates it from streaming transcription session callbacks.
- `VoiceInk/Views/ModelSettingsView.swift`: describes the setting as applying only to real-time streaming models.

## Windows Scope

Implemented in this slice:

- Add `ShowLiveTranscriptPreview` to Windows `AppSettings`, JSON persistence, and settings backup/export/import as part of General Settings.
- Add `PartialTranscript` state to `DictationController`, with an update method for future streaming adapters and automatic clearing on start/stop/cancel/error boundaries.
- Extend `FloatingRecorderPresenter` and `FloatingRecorderViewState` so live text is visible only when:
  - dictation state is `Recording`,
  - the setting is enabled,
  - partial transcript text is non-empty after trimming.
- Add a compact live transcript panel above the Windows floating recorder controls. It expands the recorder window upward and stays inside the existing no-activate recorder window.
- Add an AI Models setting named `Show Live Transcript Preview`, matching the macOS location and language.

Not implemented in this slice:

- Streaming transcription providers or local Whisper streaming.
- Fake partial transcript generation from audio levels or final transcripts.
- Notch-style recorder layout.

## Data Flow

Future streaming providers call `DictationController.UpdatePartialTranscript(...)` while the controller is recording. `MainWindow` passes `controller.PartialTranscript` and `ShowLiveTranscriptPreviewCheckBox.IsChecked` into `FloatingRecorderPresenter.FromState(...)`. The floating recorder renders the resulting view state. When the setting is off, the state is not recording, or no partial text exists, the transcript panel is collapsed.

## Safety And Privacy

Partial transcript text stays in memory and is cleared at recording boundaries. It is not added to diagnostics, settings backups, history, logs, or metrics. This slice does not send audio or transcript text to any new service.

## Verification

- Add focused Core tests for presenter gating and controller reset behavior.
- Add Infrastructure settings persistence coverage for `ShowLiveTranscriptPreview`.
- Run focused tests first, then full Windows tests and Debug x64 build after app wiring.
