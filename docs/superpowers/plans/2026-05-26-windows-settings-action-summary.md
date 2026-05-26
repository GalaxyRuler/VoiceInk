# Windows Settings Action Summary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a scan-friendly Settings action summary.

**Architecture:** Extend the Core presenter with summary rows and bind WinUI to those rows. The slice is presentation-only.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Presenter Rows

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/SettingsSectionPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Settings/SettingsSectionPresenterTests.cs`

- [x] **Step 1: Write failing tests**
- [x] **Step 2: Run focused tests to verify RED**
- [x] **Step 3: Implement summary rows**
- [x] **Step 4: Run focused tests to verify GREEN**

### Task 2: WinUI Binding

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add summary list**
- [x] **Step 2: Bind presenter rows**
- [x] **Step 3: Run focused tests and build**

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**
- [x] **Step 2: Run final verification**
- [x] **Step 3: Review and commit**
