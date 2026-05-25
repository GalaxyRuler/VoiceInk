# Windows Dedicated History Window Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a separate WinUI History window matching the macOS dedicated transcription history surface at MVP depth.

**Architecture:** Keep reusable command-state logic in Core. Add a `HistoryWindow` WinUI class that receives existing app services from `MainWindow`, and update `OpenHistoryWindowAsync` to create/focus this window while keeping the inline History page intact.

**Tech Stack:** .NET 10, C#, WinUI 3, SQLite history store, existing VoiceInk Core services.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryWindowCommandState.cs`: command availability DTO.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryWindowCommandPresenter.cs`: pure command-state presenter.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryWindowCommandPresenterTests.cs`: focused tests.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml`: dedicated window layout.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml.cs`: window logic and action handlers.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: create/focus dedicated History window.
- Modify `docs/superpowers/project-completion.md`: update History and Shell progress.

## Tasks

### Task 1: Core Command Presenter

- [ ] Write failing tests proving:
  - no selection disables all selected-row commands;
  - a completed text-only row enables copy/re-enhance/delete but not retry/open-audio/enhanced/AI-request;
  - a completed row with audio, enhanced text, and AI request enables all matching commands;
  - failed/canceled rows disable retry and re-enhance but still allow delete and copy available text.
- [ ] Run focused tests and verify RED because presenter types do not exist.
- [ ] Add the presenter and state record.
- [ ] Run focused tests and verify GREEN.

### Task 2: WinUI History Window Shell

- [ ] Add `HistoryWindow.xaml` with a left search/list pane, center transcript detail pane, and right metadata/AI request pane.
- [ ] Add constructor dependencies for existing stores/services and recordings directory.
- [ ] Implement refresh/search/clear/list-selection/detail rendering.
- [ ] Build the app project.

### Task 3: Window Actions

- [ ] Wire export CSV, retry selected, re-enhance selected, copy actions, open audio, and delete.
- [ ] Use `HistoryWindowCommandPresenter` for enablement.
- [ ] Refresh/select new items after retry and re-enhance.
- [ ] Build the app project.

### Task 4: Main Window Lifecycle

- [ ] Replace `OpenHistoryWindowAsync` inline navigation behavior with create/focus of the dedicated `HistoryWindow`.
- [ ] Keep one window instance alive and clear the reference on close.
- [ ] Ensure tray/shortcut History commands call the same method.
- [ ] Build the app project.

### Task 5: Verification and Commit

- [ ] Update completion tracker.
- [ ] Run focused tests, full solution tests, full x64 build, and `git diff --check`.
- [ ] Review for accidental commercial surfaces, unrelated changes, and unstaged `.superpowers/`.
- [ ] Commit with `feat(windows): add dedicated history window`.
