# Windows Stale Model Cleanup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a safe stale imported model cleanup action to the AI Models page.

**Architecture:** Core owns model list filtering with injected file delegates. WinUI adds one action button and clears the selected model path only when that selected path was removed.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Cleanup Helper

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/LocalWhisperModelService.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/LocalWhisperModelServiceTests.cs`

- [x] Add failing tests for removing unavailable imported models and preserving usable models.
- [x] Run focused model tests.
- [x] Implement cleanup helper.
- [x] Rerun focused model tests.

### Task 2: WinUI Cleanup Action

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] Add `Remove Unavailable Models` action beside import/use/open downloads.
- [x] Wire the action to Core cleanup, save settings, refresh choices, and clear the current model path when needed.
- [x] Disable the action when no imported model is unavailable.
- [x] Run Debug x64 build.

### Task 3: Docs, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Document stale imported model cleanup.
- [x] Run full solution tests.
- [x] Run Debug x64 build.
- [x] Run `git diff --check`.
- [x] Commit with `feat(windows): clean up stale imported models`.
