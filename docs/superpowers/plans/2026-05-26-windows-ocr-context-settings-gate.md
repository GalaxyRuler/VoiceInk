# Windows OCR Context Settings Gate Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a default-off settings gate and UI checkbox for OCR enhancement context.

**Architecture:** Store the gate in `AppSettings`, make `TextEnhancementPipeline` derive `IncludeOcr` from that setting, then bind the WinUI Enhancement checkbox into the existing load/save/enable state flow.

**Tech Stack:** .NET 10, C#, xUnit, WinUI 3 XAML.

---

### Task 1: Core Settings Gate

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/TextEnhancementPipeline.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/TextEnhancementPipelineTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`

- [x] **Step 1: Write failing tests**

Add tests proving default settings do not request OCR, enabled settings do request and render OCR, and JSON settings persist `UseOcrContext`.

- [x] **Step 2: Verify red**

Run focused Core and settings tests. Expected: fail because `UseOcrContext` does not exist and the pipeline always requests OCR.

- [x] **Step 3: Implement minimal Core/settings code**

Add `UseOcrContext` to settings equality/hash, persist via normal JSON serialization, and pass `settings.UseOcrContext` into `EnhancementContextRequest.IncludeOcr`.

- [x] **Step 4: Verify green**

Run the same focused tests. Expected: pass.

### Task 2: WinUI Checkbox Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add checkbox**

Place `UseOcrContextCheckBox` next to `UseClipboardContextCheckBox` in the Enhancement section.

- [x] **Step 2: Wire load/save/enabled state**

Set the checkbox from `settings.UseOcrContext`, save it into `AppSettings`, and enable it with enhancement controls.

- [x] **Step 3: Build**

Run x64 Debug build. Expected: build succeeds.

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update completion bar**

Mark the settings gate slice complete and note that real capture/OCR remains.

- [x] **Step 2: Run full verification**

Run full solution tests and x64 Debug build.

- [ ] **Step 3: Review and commit**

Request review, fix Critical/Important issues, then commit.
