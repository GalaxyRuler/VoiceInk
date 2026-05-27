# Windows Power Mode Persist Preferences Spec

## Source of Truth

- `VoiceInk/Views/Settings/SettingsView.swift` exposes `Persist Configured Preferences` under the Power Mode settings section.
- `VoiceInk/Transcription/Engine/VoiceInkEngine.swift` clears the active Power Mode after recorder cleanup unless `powerModePersistConfig` is enabled.
- Public VoiceInk Power Mode documentation describes manual mini-recorder/shortcut Power Mode switching as an in-recording workflow.

## Windows Behavior

- Add a free/open-source local setting named `PersistPowerModeSelection`.
- Default remains `false`, matching macOS `powerModePersistConfig`.
- When disabled, a manually selected Power Mode rule is cleared after recorder stop or cancel so the next recording returns to Auto matching/default settings.
- When enabled, the selected rule remains active across recorder sessions.
- Power Mode matching still honors the master `IsPowerModeEnabled` setting.
- Backup/export and JSON settings persistence include the new setting.
- The WinUI Power Mode page shows a checkbox labeled `Persist Configured Preferences` near the master Power Mode toggle.

## Non-Goals

- No commercial, account, license, or telemetry behavior.
- No destructive migration of existing rules; only transient selected-rule cleanup is added.
