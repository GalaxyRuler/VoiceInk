# Windows History Copy Actions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add one-click copy actions for selected History transcript text and AI request details.

**Architecture:** Keep text selection rules in Core with a tiny pure selector. Wire WinUI buttons to that selector and the existing clipboard copy service.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryCopyTextKind.cs`: enum for `Original`, `Final`, `Enhanced`, and `AiRequest`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryCopyTextResult.cs`: selector result with `Success`, `Message`, and `Text`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryCopyTextSelector.cs`: pure selection logic.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryCopyTextSelectorTests.cs`: focused tests.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add copy buttons near the History detail fields.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: wire handlers, enablement, and clipboard copy calls.
- Modify `docs/superpowers/project-completion.md`: update the current slice and History completion.

## Tasks

### Task 1: Tests

- [ ] Write tests that assert:
  - `Original` returns `OriginalText`.
  - `Final` returns `Text`.
  - `Enhanced` returns `EnhancedText` and fails when absent.
  - `AiRequest` combines system and user messages with labels.
  - blank selected text fails with a clear message.
- [ ] Run the focused test and verify RED because selector types do not exist.

### Task 2: Core Selector

- [ ] Add the enum, result record, and selector.
- [ ] Keep the selector pure and deterministic.
- [ ] Run focused tests and verify GREEN.

### Task 3: WinUI Wiring

- [ ] Add `Copy Original`, `Copy Final`, `Copy Enhanced`, and `Copy AI Request` buttons in History detail.
- [ ] Add click handlers that call the selector and `textInjectionService.CopyAsync`.
- [ ] Enable buttons only when the selected history row has text for that kind.
- [ ] Build the app project.

### Task 4: Verification and Commit

- [ ] Update the completion tracker.
- [ ] Run focused tests, full solution tests, full x64 build, and `git diff --check`.
- [ ] Review staged changes.
- [ ] Commit with `feat(windows): add history copy actions`.
