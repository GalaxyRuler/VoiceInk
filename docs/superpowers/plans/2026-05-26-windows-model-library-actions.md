# Windows Model Library Actions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development for presenter behavior and superpowers:verification-before-completion before committing. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add presenter-backed AI Models action rows for local Whisper catalog download, import, default model, repair, and warmup.

**Architecture:** Extend `ModelLibraryOverviewPresenter` to return action rows derived from existing model library inputs. Bind the AI Models page to those rows during model catalog refresh.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Action Rows

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/ModelLibraryOverviewPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/ModelLibraryOverviewPresenterTests.cs`

- [x] **Step 1: Write failing presenter tests**
- [x] **Step 2: Run focused tests to verify RED**
- [x] **Step 3: Implement model action rows**
- [x] **Step 4: Run focused tests to verify GREEN**

### Task 2: WinUI Binding

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add model action list**
- [x] **Step 2: Bind model action rows during catalog refresh**
- [x] **Step 3: Run focused tests and build**

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update parity docs**
- [x] **Step 2: Run final verification**
- [x] **Step 3: Review and commit**
