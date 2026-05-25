# Windows OCR Region Controls Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add persisted WinUI controls for optional Screen OCR Context capture bounds.

**Architecture:** Store OCR region values in Core settings, have a Native settings-backed OCR reader resolve the region at OCR time, and expose WinUI `NumberBox` controls in the Enhancement page. Keep OCR disabled by default and full-screen by default when no region is enabled.

**Tech Stack:** .NET 10, WinUI 3 `NumberBox`, System.Text.Json settings persistence, xUnit.

---

### Task 1: Persist Region Settings

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`

- [x] **Step 1: Write failing persistence test**

Add `UseOcrCaptureRegion`, `OcrCaptureRegionLeft`, `OcrCaptureRegionTop`, `OcrCaptureRegionWidth`, and `OcrCaptureRegionHeight` to the settings persistence test.

- [x] **Step 2: Verify red**

Focused JSON settings tests failed because the fields did not exist.

- [x] **Step 3: Implement settings fields**

Add the fields to `AppSettings` equality and hash code.

### Task 2: Settings-Backed OCR Reader

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/SettingsBackedOcrTextReader.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text/SettingsBackedOcrTextReaderTests.cs`

- [x] **Step 1: Write failing reader tests**

Assert the reader passes `null` region when region mode is off, passes saved bounds when region mode is on, and skips recognition for empty saved bounds through `WindowsScreenOcrTextReader`.

- [x] **Step 2: Implement reader**

Load `AppSettings` from `ISettingsStore` at OCR time and delegate to `WindowsScreenOcrTextReader`.

### Task 3: WinUI Controls

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add controls**

Add a checkbox and four `NumberBox` controls for OCR region bounds.

- [x] **Step 2: Wire settings**

Load, save, enable, and use the controls. Construct `WindowsEnhancementContextProvider` with `SettingsBackedOcrTextReader`.

- [x] **Step 3: Fix review finding**

Added validation so enabled OCR region mode refuses invalid numeric values and non-positive width/height before settings save.

### Task 4: Verification And Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update tracker**

Recorded visible OCR region controls as completed and left overlay picker as future polish.

- [x] **Step 2: Verify and review**

Ran focused tests, full solution tests, x64 build, and review. Fixed Important review feedback by validating OCR region number boxes before saving.

- [x] **Step 3: Commit**

Commit only intentional source/docs changes.
