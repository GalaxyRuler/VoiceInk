# Windows Model Health Guidance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add actionable local Whisper model health guidance to the Windows AI Models page.

**Architecture:** Add a Core `LocalWhisperModelHealthPresenter` that maps existing health states to title/guidance/action records. WinUI assigns the presenter guidance under the existing default model status.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Health Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/LocalWhisperModelHealthPresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/LocalWhisperModelHealthPresenterTests.cs`

- [x] **Step 1: Write failing tests**
- [x] **Step 2: Run focused tests to verify RED**
- [x] **Step 3: Implement the presenter**
- [x] **Step 4: Run focused tests to verify GREEN**

### Task 2: WinUI Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add model repair hint text block**
- [x] **Step 2: Assign presenter guidance when model health is refreshed**
- [x] **Step 3: Build**

### Task 3: Docs, Verification, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**
- [ ] **Step 2: Run full tests, build, and diff check**
- [ ] **Step 3: Review and commit**
