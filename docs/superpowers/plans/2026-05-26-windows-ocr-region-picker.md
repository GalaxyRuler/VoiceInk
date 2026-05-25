# Windows OCR Region Picker Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a visual OCR capture region picker that persists into the existing OCR region settings.

**Architecture:** Keep geometry in `VoiceInk.Windows.Native` beside `ScreenCaptureRegion` so it can be tested without UI automation. Add a transient WinUI `OcrRegionPickerWindow` in the app project and wire it from the Enhancement page.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK `AppWindow`/`FullScreenPresenter`, xUnit.

---

### Task 1: Document picker behavior

**Files:**
- Create: `docs/superpowers/specs/2026-05-26-windows-ocr-region-picker-design.md`
- Create: `docs/superpowers/plans/2026-05-26-windows-ocr-region-picker.md`

- [x] **Step 1: Save design and execution plan**

Record behavior, non-goals, verification, and the touched files.

### Task 2: Add region selection geometry tests

**Files:**
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text/ScreenCaptureRegionSelectionTests.cs`

- [x] **Step 1: Add failing tests**

Cover normalized reverse drags, display origin offsets, rasterization scale conversion, and tiny drag cancellation.

- [x] **Step 2: Run focused tests and confirm failure**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter ScreenCaptureRegionSelectionTests
```

Expected: compile failure because `ScreenCaptureRegionSelection` does not exist yet.

### Task 3: Implement geometry helper

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ScreenCaptureRegionSelection.cs`

- [x] **Step 1: Add `ScreenCaptureRegionSelection.FromDrag`**

Normalize start/end points, apply rasterization scale, add display origin, and return `null` for tiny selections.

- [x] **Step 2: Run focused tests and confirm pass**

Run the focused test command above.

### Task 4: Add and wire picker UI

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.App/OcrRegionPickerWindow.xaml`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.App/OcrRegionPickerWindow.xaml.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add full-screen picker window**

Use a dark translucent root, an instruction strip, a bordered rectangle, pointer press/move/release handlers, Escape cancel, and `FullScreenPresenter`.

- [x] **Step 2: Add Enhancement page button**

Place `Select Region` beside the OCR region toggle and enable it only when OCR region controls are enabled.

- [x] **Step 3: Save picker result**

On successful selection, check OCR context and constrained region, fill numeric fields, call `ApplyEnhancementSettingsAsync`, and show a status message.

- [x] **Step 4: Build app**

Run Debug x64 build.

### Task 5: Verify, document, and commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update docs and completion tracker**

Mention the visual picker and raise Context features modestly.

- [x] **Step 2: Run verification**

Run focused geometry tests, full solution tests, Debug x64 build, `git diff --check`, and review.

- [x] **Step 3: Commit**

Commit with:

```powershell
git commit -m "feat(windows): add ocr region picker"
```
