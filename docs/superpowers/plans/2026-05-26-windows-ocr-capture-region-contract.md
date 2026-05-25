# Windows OCR Capture Region Contract Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add region-aware screen capture plumbing for future OCR picker UI.

**Architecture:** Extend the native OCR capture interface with an optional Core-independent region record and pass it through `WindowsScreenOcrTextReader`. Keep default behavior as full virtual desktop capture.

**Tech Stack:** .NET 10, Windows Forms screen bounds, System.Drawing capture, xUnit.

---

### Task 1: Region-Aware OCR Reader

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ScreenCaptureRegion.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/IScreenImageCapture.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsScreenOcrTextReader.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text/WindowsScreenOcrTextReaderTests.cs`

- [x] **Step 1: Write failing tests**

Add tests that a configured region is passed to capture, and that empty regions skip recognition.

- [x] **Step 2: Verify red**

Focused OCR reader tests failed because `ScreenCaptureRegion` and the new capture signature did not exist.

- [x] **Step 3: Implement region plumbing**

Add `ScreenCaptureRegion`, update capture interface/reader, and preserve the full-screen default.

- [x] **Step 4: Verify focused tests**

Focused OCR reader tests passed.

### Task 2: Desktop Capture Region

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsDesktopScreenImageCapture.cs`

- [x] **Step 1: Capture requested bounds**

Use the virtual desktop bounds when no region exists, otherwise capture the requested rectangle. Return empty bytes for non-positive dimensions.

- [x] **Step 2: Verify native build**

Native project x64 build passed.

### Task 3: Docs And Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update tracker**

Recorded OCR region plumbing as complete while leaving visible picker UI as remaining work.

- [x] **Step 2: Full verification and commit**

Ran focused tests, full solution tests, x64 build, local review, and committed the slice.
