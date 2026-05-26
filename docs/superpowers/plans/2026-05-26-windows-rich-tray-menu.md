# Windows Rich Tray Menu Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style quick-setting submenus to the Windows tray icon.

**Architecture:** Keep menu state records in Core, native rendering in `VoiceInk.Windows.Native`, and command handling in the WinUI shell. The tray service raises typed option events; `MainWindow` updates existing controls/settings so the current persistence and validation paths remain authoritative.

**Tech Stack:** .NET 10, WinUI 3, Windows Forms `NotifyIcon`, xUnit.

---

### Task 1: Core Tray State

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayShellState.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayShellPresenter.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayMenuOption.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayQuickSettingsState.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shell/TrayShellPresenterTests.cs`

- [x] Add a failing test for `CanUseQuickSettings`.
- [x] Add quick-settings enablement and reusable tray option state.
- [x] Run targeted Core tray tests.

### Task 2: Native Tray Menu

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Tray/TrayIconService.cs`

- [x] Add nested submenus for model/provider/language/enhancement/context/audio/Power Mode.
- [x] Add typed events for tray option selection and context toggles.
- [x] Keep existing tray commands and tooltip behavior.

### Task 3: App Wiring And Docs

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Build tray quick-setting state from existing app state.
- [x] Route tray selections through existing settings, validation, warmup, and navigation paths.
- [x] Update docs and completion tracker.
- [x] Run full tests, Debug x64 build, `git diff --check`, review, and commit.
