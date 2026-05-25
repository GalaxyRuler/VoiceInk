# Windows Screen OCR Reader Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a real local Windows OCR reader behind the default-off OCR context gate.

**Architecture:** Add small native interfaces for screen snapshot capture and OCR recognition, compose them in `WindowsScreenOcrTextReader`, and wire the default provider to use it. The provider remains responsible for graceful failure.

**Tech Stack:** .NET 10, C#, xUnit, System.Drawing/Windows Forms screen snapshot, Windows.Media.Ocr.

---

### Task 1: Testable OCR Reader Orchestration

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/IScreenImageCapture.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/IOcrTextRecognizer.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsScreenOcrTextReader.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text/WindowsScreenOcrTextReaderTests.cs`

- [x] **Step 1: Write failing tests**

Test that empty capture skips recognition, recognized text is trimmed/capped, and OCR reader calls capture before recognition.

- [x] **Step 2: Verify red**

Run focused Infrastructure OCR reader tests. Expected: fail because the new classes do not exist.

- [x] **Step 3: Implement reader orchestration**

Add the interfaces and reader class.

- [x] **Step 4: Verify green**

Run focused Infrastructure OCR reader tests. Expected: pass.

### Task 2: Windows Capture And OCR Implementations

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsDesktopScreenImageCapture.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsMediaOcrTextRecognizer.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsEnhancementContextProvider.cs`

- [x] **Step 1: Implement desktop PNG capture**

Capture the virtual desktop bounds into a PNG byte array.

- [x] **Step 2: Implement Windows.Media.Ocr recognizer**

Decode PNG bytes into a `SoftwareBitmap`, create the user-profile OCR engine, and join recognized lines.

- [x] **Step 3: Wire default provider**

Replace `EmptyOcrTextReader` in the default constructor with `WindowsScreenOcrTextReader`.

- [x] **Step 4: Build**

Run x64 Debug build. Expected: compile cleanly.

### Task 3: Verification, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Run full tests and build**

Run full solution tests and x64 Debug build.

- [x] **Step 2: Request review**

Fix Critical/Important findings. External subagent review was unavailable because the agent thread limit was reached, so this slice used a local diff review after full verification.

- [ ] **Step 3: Commit**

Commit the reader slice.
