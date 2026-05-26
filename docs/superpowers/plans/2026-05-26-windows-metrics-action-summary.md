# Windows Metrics Action Summary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development for presenter behavior and superpowers:verification-before-completion before committing. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add presenter-backed Metrics action rows for filter, export, model performance, and reset scope.

**Architecture:** Extend `SessionMetricsDashboardPresenter` and bind the Metrics page to the new rows. Leave storage/export/reset behavior unchanged.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Metrics Action Rows

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsDashboardPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/SessionMetricsDashboardPresenterTests.cs`

- [x] **Step 1: Write failing presenter tests**
- [x] **Step 2: Run focused tests to verify RED**
- [x] **Step 3: Implement action rows**
- [x] **Step 4: Run focused tests to verify GREEN**

### Task 2: WinUI Binding

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add metrics action list**
- [x] **Step 2: Bind action rows during metrics refresh**
- [x] **Step 3: Run focused tests and build**

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update parity docs**
- [x] **Step 2: Run final verification**
- [x] **Step 3: Review and commit**
