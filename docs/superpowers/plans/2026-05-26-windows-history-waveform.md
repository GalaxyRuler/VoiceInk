# Windows History Waveform Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add dependency-free waveform visualization to History audio playback.

**Architecture:** Parse PCM WAV peaks in Core and render normalized doubles as lightweight WinUI bars in the inline and dedicated History audio panels.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryWaveformPeakExtractor.cs`: RIFF/WAVE PCM16 parser and peak extractor.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryWaveformPeakExtractorTests.cs`: focused parser tests.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.App/WaveformPeakHeightConverter.cs`: maps normalized peak values to bar heights.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add inline History waveform `ItemsControl`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: load peaks for selected audio.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml`: add dedicated window waveform `ItemsControl`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml.cs`: load peaks for selected audio.
- Modify `docs/superpowers/project-completion.md`: update History progress and current slice.

## Tasks

### Task 1: Core Peak Extractor

- [ ] Write failing tests for invalid bytes, silent PCM, mono peaks, stereo peaks, and target count.
- [ ] Run focused tests and verify RED.
- [ ] Implement `HistoryWaveformPeakExtractor`.
- [ ] Run focused tests and verify GREEN.

### Task 2: WinUI Bar Rendering

- [ ] Add `WaveformPeakHeightConverter`.
- [ ] Add waveform `ItemsControl` resources and panels to both History audio surfaces.
- [ ] Load waveform peaks from selected audio files and hide the panel when none are available.
- [ ] Build the app project.

### Task 3: Verification and Commit

- [ ] Update completion tracker.
- [ ] Run focused tests, full solution tests, full x64 build, and `git diff --check`.
- [ ] Review for accidental commercial surfaces and unrelated changes.
- [ ] Commit with `feat(windows): add history waveform visualization`.
