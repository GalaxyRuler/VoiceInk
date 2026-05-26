# Windows Shortcut Recorder UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add explicit shortcut recorder affordances to Settings and Power Mode shortcut fields.

**Architecture:** Keep existing shortcut parsing/capture logic. WinUI adds read-only capture fields and record buttons that focus the relevant field.

**Tech Stack:** .NET 10, WinUI 3.

---

### Task 1: Settings Shortcut Recorder Controls

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] Add `Record` buttons for every Settings shortcut field.
- [x] Make Settings shortcut fields read-only capture targets.
- [x] Add a shared `ShortcutRecorderButton_Click` handler that focuses/selects the target field.

### Task 2: Power Mode Shortcut Recorder Control

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`

- [x] Add the same read-only capture target and `Record` button affordance for the Power Mode rule shortcut.

### Task 3: Docs And Verification

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Update docs and completion tracker.
- [x] Run full tests, Debug x64 build, `git diff --check`, and commit.
