# History Export Picker Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let Windows users choose where to save history CSV exports.

**Architecture:** Reuse the existing Core `HistoryCsvExporter` and current shell history list/search state. Keep picker integration in the WinUI app layer and initialize the picker with this window's HWND as required for desktop WinUI apps. Keep the previous local export folder only as a fallback if picker initialization or write fails in an environment where Windows file pickers are unavailable.

**Tech Stack:** .NET 10, WinUI 3, Windows.Storage.Pickers `FileSavePicker`, `WinRT.Interop.InitializeWithWindow`, existing `HistoryCsvExporter`.

---

## Source Notes

- macOS source of truth: `VoiceInk/Services/VoiceInkCSVExportService.swift` and `VoiceInk/Views/History/InlineHistoryView.swift` provide history export from selected/current history UI.
- Windows docs reference: Microsoft Learn file picker guidance says desktop WinUI apps must associate pickers with the owner window handle; Microsoft Learn also documents Windows App SDK picker APIs for desktop apps.

## File Structure

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: use `FileSavePicker` for CSV export and write the current history/search results to the selected file.
- Modify `README.md`: note picker-based CSV export.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`: move picker export from gap to implemented.
- Modify this plan with verification status.

## Task 1: Picker Export

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Implement picker export**

Implementation rules:

- Ensure the current history list is populated before export.
- Create `FileSavePicker`.
- Set `SuggestedStartLocation = PickerLocationId.DocumentsLibrary`.
- Add file type choice `CSV file` with `.csv`.
- Set suggested file name `VoiceInk-history-yyyyMMdd-HHmmss`.
- Initialize with `InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this))`.
- If the user cancels, report `History export canceled`.
- Write `HistoryCsvExporter.Export(historyItems)` to the selected file with `FileIO.WriteTextAsync`.
- Report `History exported: <file name>`.

- [x] **Step 2: Build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [x] **Step 3: Commit**

Commit message:

```text
feat(windows): choose history csv export path
```

## Task 2: Docs, Review, And Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/plans/2026-05-24-history-export-picker.md`

- [ ] **Step 1: Update docs**

Document picker-based CSV export. Keep batch actions, audio playback, and retry-last as gaps.

- [ ] **Step 2: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [ ] **Step 3: Run Debug x64 build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [ ] **Step 4: Request review and fix Important findings**

Review the slice from this plan commit through HEAD. Fix Critical and Important findings before proceeding.

- [ ] **Step 5: Commit docs**

Commit message:

```text
docs(windows): note picker-based history export
```

## Plan Self-Review

Spec coverage:

- Covers picker-based CSV export for the current history/search result set.
- Does not cover batch-selected export because selection checkboxes do not exist yet.

Placeholder scan:

- No placeholder sections remain.

Type consistency:

- Uses existing `HistoryCsvExporter`, `historyItems`, and `ExportHistoryAsync` names.
