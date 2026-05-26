# Windows Rich Tray Menu Design

## Goal

Bring the Windows tray menu closer to the macOS menu-bar shell by exposing quick access to the same dictation controls users expect while VoiceInk runs in the background.

## Grounding

- macOS `MenuBarView.swift` exposes recorder toggle, transcription model, AI enhancement toggle, prompt, AI provider/model, language, audio input, context toggles, retry/copy/history/settings, launch-at-login, help, and quit.
- Windows already uses `System.Windows.Forms.NotifyIcon` with `ContextMenuStrip`, which supports nested `ToolStripMenuItem` submenus and check states.
- Windows App SDK/WinUI does not provide a first-class notification-area menu API, so the existing Win32/WinForms tray bridge remains the appropriate platform adaptation.

## Behavior

- Keep existing Show, Hide, Start/Stop Recording, Quick Add, History, and Quit commands.
- Add tray submenus for transcription model, transcription provider, language, AI prompt, AI provider, Power Mode, and audio input.
- Add tray checkable toggles for AI Enhancement, Clipboard Context, and Context Awareness.
- Add navigation commands for Manage Models, Enhancement Settings, Audio Input Settings, and Settings.
- Disable quick-setting submenus while settings are loading, recording is active, or post-recording work is busy.
- Use current Windows source state for imported models, provider presets, prompt catalog, language choices, audio choices, and enabled Power Mode rules.

## Open-Source Adaptation

- Do not port macOS commercial update, support, purchase, or Pro commands.
- Keep Settings and diagnostics as local open-source routes.

## Verification

- Add Core presenter coverage for quick-settings enablement.
- Build Debug x64 to verify the native tray event surface and WinUI code-behind wiring.
- Run full solution tests and `git diff --check` before commit.
