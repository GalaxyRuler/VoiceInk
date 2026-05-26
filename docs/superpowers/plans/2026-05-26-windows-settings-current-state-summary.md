# Windows Settings Current State Summary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development for presenter behavior and superpowers:verification-before-completion before committing. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add presenter-backed Settings current-state summary rows for paste, clipboard, recording feedback, and privacy cleanup.

**Architecture:** Extend `SettingsSectionPresenter` to accept `AppSettings` and return a new row collection. Bind the WinUI Settings page to those rows from the settings already loaded into the UI.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Presenter Rows

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/SettingsSectionPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Settings/SettingsSectionPresenterTests.cs`

- [x] **Step 1: Write failing presenter tests**
- [x] **Step 2: Run focused tests to verify RED**
- [x] **Step 3: Implement current-state rows**
- [x] **Step 4: Run focused tests to verify GREEN**

### Task 2: WinUI Binding

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add Settings current-state list**
- [x] **Step 2: Bind current settings presentation**
- [x] **Step 3: Run focused tests and build**

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update parity docs**
- [x] **Step 2: Run final verification**
- [x] **Step 3: Review and commit**
