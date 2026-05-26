# Windows OCR Display Picker Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:test-driven-development for the display catalog and superpowers:verification-before-completion before committing. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a display selector for the Windows OCR region picker.

**Architecture:** Keep absolute OCR region persistence unchanged. Add display enumeration to `VoiceInk.Windows.Native`, bind it in the WinUI Enhancement page, and pass the selected display bounds into the picker window.

**Tech Stack:** .NET 10, C#, WinUI 3, Windows Forms screen enumeration, xUnit.

---

### Task 1: Display Catalog

**Files:**
- Add/modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/*`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text/ScreenCaptureRegionSelectionTests.cs`

- [x] **Step 1: Write failing display catalog tests**
- [x] **Step 2: Run focused tests to verify RED**
- [x] **Step 3: Implement display records and catalog**
- [x] **Step 4: Run focused tests to verify GREEN**

### Task 2: Picker Targeting

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/OcrRegionPickerWindow.xaml.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add OCR display selector**
- [x] **Step 2: Pass selected display to picker**
- [x] **Step 3: Run focused tests and build**

### Task 3: Docs, Review, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] **Step 1: Update docs**
- [x] **Step 2: Run final verification**
- [x] **Step 3: Review and commit**
