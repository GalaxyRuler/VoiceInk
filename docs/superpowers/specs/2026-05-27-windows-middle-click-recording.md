# Windows Middle-Click Recording

## Goal

Port the macOS optional middle-click recording trigger to Windows as a default-off, configurable recording shortcut.

## Source Of Truth

- macOS defaults: `VoiceInk/AppDefaults.swift`
- macOS settings UI: `VoiceInk/Views/Settings/SettingsView.swift`
- macOS shortcut behavior: `VoiceInk/Shortcuts/RecordingShortcutManager.swift`
- Windows hotkey integration: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Hotkeys/GlobalHotkeyService.cs`
- Windows settings page: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Win32 grounding: Microsoft documents `WH_MOUSE_LL` as the global low-level mouse hook and `SetWindowsHookEx` as the installation API.

## Requirements

- Add `IsMiddleClickRecordingEnabled`, default false.
- Add `MiddleClickActivationDelayMilliseconds`, default 200.
- Persist both settings through JSON settings and settings backup/import.
- Expose both controls in the Windows Shortcuts settings area.
- Apply middle-click settings through the existing Apply Shortcuts flow.
- When enabled, start a delay when the middle mouse button is pressed.
- If the middle mouse button is released before the delay, cancel the trigger.
- If the delay completes, raise a recording toggle action using toggle mode.
- Do not suppress, modify, or consume mouse input.
- Unhook cleanly when shortcuts are re-registered or the app exits.
- If hook installation fails, surface the existing global shortcut error path and restore previous settings when available.

## Non-Goals

- Do not add right-click, side-button, or arbitrary mouse shortcut recording in this slice.
- Do not make middle-click push-to-talk or hybrid; it matches the macOS toggle-mini-recorder behavior.
- Do not add commercial telemetry or analytics.
