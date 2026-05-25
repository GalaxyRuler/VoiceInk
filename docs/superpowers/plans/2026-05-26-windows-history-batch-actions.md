# Windows History Batch Actions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add multiple selection with batch export/delete actions to the dedicated History window.

**Architecture:** Keep batch command availability in Core. Use stable WinUI row objects carrying history ids so `SelectedItems` maps back to selected history rows without relying on duplicate display strings.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryWindowBatchCommandState.cs`: batch action state DTO.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryWindowBatchCommandPresenter.cs`: pure selection presenter.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryWindowBatchCommandPresenterTests.cs`: focused tests.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml`: enable multiple selection and add batch toolbar buttons.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/HistoryWindow.xaml.cs`: stable row objects, selected id mapping, export/delete selected, select all, clear selection.
- Modify `docs/superpowers/project-completion.md`: update the current slice and History progress.

## Tasks

### Task 1: Core Batch Presenter

- [ ] Write failing tests for no rows, no selection, partial selection, and all selected.
- [ ] Run focused tests and verify RED.
- [ ] Add presenter/state records.
- [ ] Run focused tests and verify GREEN.

### Task 2: HistoryWindow Multiple Selection

- [ ] Add stable list row objects with `Id` and `DisplayText`.
- [ ] Set `SelectionMode="Multiple"` and `DisplayMemberPath="DisplayText"`.
- [ ] Preserve selection ids across refresh and render the first selected row in details.
- [ ] Build the app project.

### Task 3: Batch Actions

- [ ] Add Select All, Clear Selection, Export Selected, and Delete Selected buttons.
- [ ] Export selected rows to CSV through the existing picker.
- [ ] Delete selected rows after confirmation and clean app-owned audio files best-effort.
- [ ] Build the app project.

### Task 4: Verification and Commit

- [ ] Update the completion tracker.
- [ ] Run focused tests, full solution tests, full x64 build, and `git diff --check`.
- [ ] Review for accidental commercial surfaces and unrelated changes.
- [ ] Commit with `feat(windows): add history batch actions`.
