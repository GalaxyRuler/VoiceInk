# Windows History Waveform Design

## Context

The macOS History audio player shows a richer playback surface than the Windows player. Windows now has playback rate controls, but still lacks a waveform visualization. The app records and imports WAV files into app-owned storage, so a dependency-free PCM WAV peak extractor can power a lightweight waveform.

Online grounding: WAVE files use RIFF chunks with little-endian `fmt ` and `data` chunks. PCM 16-bit samples are signed little-endian values, which is enough for the app-owned WAV files produced by the Windows recording/import pipeline.

## Goals

- Extract normalized waveform peaks from PCM 16-bit WAV files.
- Render waveform bars in inline History and the dedicated History window when audio exists.
- Gracefully hide the waveform for missing, unsupported, or unreadable files.
- Avoid new dependencies.
- Keep parsing logic in Core and test it directly.

## Non-goals

- Do not add waveform scrubbing in this slice.
- Do not decode compressed audio formats.
- Do not add external native audio visualization libraries.
- Do not add telemetry, accounts, or commercial features.

## Architecture

Add `HistoryWaveformPeakExtractor` in Core. It accepts WAV bytes and a target peak count, validates RIFF/WAVE structure, parses `fmt ` and `data` chunks, supports PCM 16-bit mono/stereo/multi-channel frames, and returns 0.0-1.0 peak values. WinUI reads the selected audio file bytes, maps peaks to simple vertical bars, and hides the waveform if extraction returns no peaks.

## Verification

- Core tests cover invalid data, silent PCM, mono peaks, stereo peaks, and target-count limiting.
- App project builds after adding waveform XAML/converter.
- Full solution tests/build pass before commit.
