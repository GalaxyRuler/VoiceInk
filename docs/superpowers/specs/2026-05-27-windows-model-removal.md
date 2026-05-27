# Windows Model Removal

## Goal

Add macOS-style local model removal to the Windows AI Models page while respecting Windows data-safety boundaries.

## Source Of Truth

- `VoiceInk/Views/AI Models/ModelManagementView.swift` exposes `Delete Model` for downloaded Whisper models.
- Windows stores app-downloaded GGML models under `%LocalAppData%\VoiceInk.Windows\Models`.
- Microsoft documents local app data as app-owned storage; externally imported user-selected files should not be deleted casually.
- WinUI confirmation dialogs should give users a safe chance to cancel destructive actions.

## Requirements

- Add a Core removal planner that removes a model reference and decides whether the underlying file is app-owned.
- App-owned files under the Windows models directory may be deleted after confirmation.
- Externally imported files must only be removed from VoiceInk's imported-model list; the file remains on disk.
- Removing the current default model clears the default model path.
- Add AI Models page actions for deleting a downloaded catalog model and removing the selected imported/current model reference.
- Keep all deletion behind user confirmation and do not delete directories.

## Non-Goals

- No bulk delete of all models.
- No deletion of external imported files.
- No secure wipe.
