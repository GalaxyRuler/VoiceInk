# Windows History Analysis Overlay Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development for presenter behavior and superpowers:verification-before-completion before committing. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add selected-history analysis rows.

**Architecture:** Keep analysis in Core presenter data, then bind it into the existing WinUI History detail pane.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Analysis Presenter

**Files:**
- Add: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryAnalysisPresenter.cs`
- Add: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryAnalysisPresenterTests.cs`

- [x] **Step 1: Write failing presenter tests**
- [x] **Step 2: Run focused tests to verify RED**
- [x] **Step 3: Implement analysis rows**
- [x] **Step 4: Run focused tests to verify GREEN**

### Task 2: WinUI Detail Binding

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add analysis list to History details**
- [x] **Step 2: Bind selected history analysis rows**
- [x] **Step 3: Run focused tests and build**

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**
- [x] **Step 2: Run final verification**
- [x] **Step 3: Review and commit**
