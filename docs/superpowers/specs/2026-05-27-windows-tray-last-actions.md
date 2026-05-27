# Windows Tray Last Actions Spec

## Goal

Add direct notification-area menu actions for the macOS menu-bar "Retry Last Transcription" and "Copy/Paste Last Transcription" workflow.

## Source Of Truth

- The macOS `MenuBarView` exposes `Retry Last Transcription`, `Copy Last Transcription`, `History`, `Settings`, and `Quit VoiceInk` directly in the menu-bar menu.
- The Windows fork already supports paste-last-original, paste-last-enhanced, and retry-last through shortcuts and main-window buttons.
- Windows notification-area menus should expose common background app actions without requiring the main window to be opened first.

## Requirements

- The Windows tray menu must include:
  - `Paste Last Transcription`
  - `Paste Last Enhanced`
  - `Retry Last Transcription`
- These rows must call the existing `PasteLastAsync(LastTranscriptionTextKind.Final)`, `PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred)`, and `RetryLastHistoryAsync()` flows.
- Core tray state must expose enablement for the three rows so UI behavior is testable without native tray automation.
- Paste-last tray rows must be enabled when settings are loaded, no operation is active, and the controller is not recording.
- Retry-last tray row must be disabled while the controller is busy transcribing/inserting and while recording.
- The slice must not add a new history query path or duplicate last-transcription business logic.

## Non-Goals

- No GUI automation on the active desktop.
- No changes to global shortcut registration.
- No commercial support, telemetry, or account flow.
