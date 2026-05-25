# Windows Recorder Style Design

## Goal

Add macOS-style recorder style selection to the Windows app and implement a Windows adaptation of the original notch recorder. Users should be able to choose between the existing bottom mini recorder and a top-center notch recorder without changing dictation behavior.

## Source Of Truth

macOS references inspected:

- `VoiceInk/Views/Settings/SettingsView.swift`: exposes `Recorder Style` in the Interface section with `Notch` and `Mini` segmented choices.
- `VoiceInk/Transcription/Engine/RecorderUIManager.swift`: switches between mini and notch window managers based on `RecorderType`, defaulting to `mini`.
- `VoiceInk/Views/Recorder/MiniRecorderView.swift`: renders the bottom mini pill and expands only when live transcript preview has real partial text.
- `VoiceInk/Views/Recorder/NotchRecorderView.swift`: renders a top notch pill, expands horizontally while recording/processing, and adds a compact live text row when preview text is available.
- `VoiceInk/Views/Recorder/NotchRecorderPanel.swift`: keeps the notch recorder non-activating, always-on-top, centered at the top screen edge, and wide enough for side expansion.

Windows platform references checked:

- Microsoft Learn `AppWindow.MoveAndResize`: use the Windows App SDK windowing API to move and size the recorder window in screen coordinates.
- Microsoft Learn `Manage app windows`: `AppWindow` is the WinUI 3 top-level window abstraction and supports presenter/window management without direct HWND changes for normal movement/sizing.

## Windows Scope

Implemented in this slice:

- Add a persisted recorder style setting with macOS-compatible values:
  - `mini` is the default and preserves existing behavior.
  - `notch` enables the top-center recorder.
- Add a Settings page `Recorder Style` segmented-style selection using WinUI controls.
- Include recorder style in JSON settings and General Settings backup/export/import.
- Extend the floating recorder view state so Core decides the normalized style while WinUI only renders it.
- Adapt the existing floating recorder window:
  - `mini`: bottom-center placement and existing layout.
  - `notch`: top-center placement, black rounded notch chrome, compact controls, and popovers/live transcript expanding downward from the top edge.
- Preserve existing prompt, Power Mode, stop, cancel, live preview, elapsed, input meter, and no-activate behavior.

Not implemented in this slice:

- Real streaming partial transcript providers.
- Pixel-perfect macOS physical notch geometry, because Windows devices do not expose macOS safe-area notch metrics.
- Any commercial UI, licensing, trial, purchase, telemetry, or paid updater behavior.

## Behavior

When `RecorderStyle` is missing, blank, or unknown, Windows normalizes it to `mini`. Settings save normalized values only. Existing users therefore keep the mini recorder unless they choose `Notch`.

When the notch style is selected, the recorder opens near the current display work area top edge, centered horizontally. It remains no-activate and always-on-top. Hover/click prompt and Power Mode panels render below the notch chrome so they remain visible on top-anchored windows. If live transcript preview is active, the live transcript row appears below the notch chrome and above any popover content.

## Testing

- Add Core tests for recorder style normalization and presenter state.
- Add settings persistence and backup tests for `RecorderStyle`.
- Build after WinUI wiring to verify XAML/code-behind integration.
