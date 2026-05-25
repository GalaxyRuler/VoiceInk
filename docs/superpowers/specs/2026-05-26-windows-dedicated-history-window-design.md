# Windows Dedicated History Window Design

## Context

The macOS VoiceInk app has a separate "VoiceInk — Transcription History" window managed by `HistoryWindowController`. It keeps a dedicated searchable history workspace open independently of the main app window, with a left history list, center transcript details, and right metadata/AI request inspector. Windows currently routes the History shortcut and tray command back into the main shell's inline History section.

Online grounding: Microsoft Learn documents WinUI 3 secondary windows by creating another `Window` instance and keeping a reference while it is shown. The Windows implementation should use that native WinUI pattern rather than a non-native workaround.

## Goals

- Add a dedicated WinUI history window titled `VoiceInk - Transcription History`.
- Reuse existing local stores/services; do not duplicate persistence.
- Open/focus the existing history window when the user invokes History again.
- Show a searchable history list, selected transcript detail, metadata/AI request details, and audio playback when an audio file exists.
- Expose existing History actions from the window: refresh, search, clear, export CSV, retry selected, re-enhance selected, copy original/final/enhanced/AI request, open audio, and delete.
- Keep the existing inline History page working.
- Keep all behavior local-only and open-source.

## Non-goals

- Do not add macOS performance analysis overlays in this slice.
- Do not add multi-select batch delete/export in this slice.
- Do not add waveform/rate controls in this slice.
- Do not add commercial telemetry, paid upgrade prompts, licensing, accounts, or private update flows.

## Architecture

Add a small Core presenter, `HistoryWindowCommandPresenter`, to centralize command availability for selected rows. Add a WinUI `HistoryWindow` class that receives the already-created history store, settings store, dictionary store, transcription router, enhancement pipeline, metric store, text injection service, and recordings directory from `MainWindow`.

`MainWindow.OpenHistoryWindowAsync` should create and activate the dedicated window instead of switching to the inline History section. It should keep a private reference and clear it when the secondary window closes. The secondary window owns its transient UI state and calls the same Core services already used by the main inline History page.

## Verification

- Core presenter tests cover no-selection, selected completed text-only, selected completed with audio/enhancement/AI request, and non-completed rows.
- App project builds after adding the secondary window XAML.
- Full solution tests and x64 build pass before commit.
