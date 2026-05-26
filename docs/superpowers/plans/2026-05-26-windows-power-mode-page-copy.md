# Windows Power Mode Page Copy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style Power Mode page copy, rule counts, and empty-state guidance to Windows.

**Architecture:** Add a Core `PowerModePagePresenter` that maps rules to static page copy and counts. WinUI assigns the presenter output when the rule list refreshes.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModePagePresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/PowerMode/PowerModePagePresenterTests.cs`

- [x] Write failing tests for non-empty and empty rule collections.
- [x] Run focused tests to verify RED.
- [x] Implement presenter.
- [x] Run focused tests to verify GREEN.

### Task 2: WinUI Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] Add named header, description, count, and empty text blocks.
- [x] Assign presenter output in `RefreshPowerModeRulesListView`.
- [x] Build.

### Task 3: Docs, Verification, Commit

- [x] Update tracker/spec.
- [ ] Run full tests, build, and `git diff --check`.
- [ ] Review and commit.
