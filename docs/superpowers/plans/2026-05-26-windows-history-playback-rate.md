# Windows History Playback Rate Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add playback speed controls to History audio players.

**Architecture:** Keep supported playback-rate choices in a Core presenter. Wire simple ComboBox controls to the existing inline and dedicated History `MediaPlayerElement` playback sessions.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryPlaybackRateChoice.cs`: label/value record.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryPlaybackRatePresenter.cs`: supported choices and selection helpers.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryPlaybackRatePresenterTests.cs`: focused tests.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: inline History rate ComboBox.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: apply selected inline rate.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml`: dedicated History rate ComboBox.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml.cs`: apply selected dedicated-window rate.
- Modify `docs/superpowers/project-completion.md`: update current slice and History progress.

## Tasks

### Task 1: Core Playback Rate Presenter

- [ ] Write failing tests for supported choices, default rate, invalid-rate clamping, and selected index lookup.
- [ ] Run focused tests and verify RED.
- [ ] Add `HistoryPlaybackRateChoice` and `HistoryPlaybackRatePresenter`.
- [ ] Run focused tests and verify GREEN.

### Task 2: Inline History Player

- [ ] Add a playback-rate ComboBox beside the inline History audio player.
- [ ] Populate choices from the presenter during setup.
- [ ] Reset rate to 1.0x when a new audio file is loaded.
- [ ] Apply selected rate to `HistoryAudioPlayer.MediaPlayer.PlaybackSession.PlaybackRate`.
- [ ] Build the app project.

### Task 3: Dedicated History Window Player

- [ ] Add a playback-rate ComboBox beside the dedicated History audio player.
- [ ] Populate choices from the presenter in the window constructor.
- [ ] Reset rate to 1.0x when a new audio file is loaded.
- [ ] Apply selected rate to `AudioPlayer.MediaPlayer.PlaybackSession.PlaybackRate`.
- [ ] Build the app project.

### Task 4: Verification and Commit

- [ ] Update the completion tracker.
- [ ] Run focused tests, full solution tests, full x64 build, and `git diff --check`.
- [ ] Review for accidental commercial surfaces and unrelated changes.
- [ ] Commit with `feat(windows): add history playback rate controls`.
