# Cleanup Settings UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expose the newly wired transcription cleanup settings in the current Windows WinUI shell.

**Architecture:** Keep the existing minimal `MainWindow` composition for this slice. Bind simple WinUI controls to the already persisted `AppSettings` cleanup fields and keep the dictation pipeline unchanged.

**Tech Stack:** .NET 10, WinUI 3, existing JSON settings store.

---

## File Structure

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add cleanup controls below the model path.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: load/save cleanup settings and map punctuation cleanup selections.
- Modify `README.md`: document that cleanup settings are now available in the Windows shell.

## Task 1: Add Cleanup Controls To MainWindow

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [ ] **Step 1: Add controls**

Add checkboxes for `Remove filler words`, `Lowercase transcription`, and `Append trailing space`, plus a `ComboBox` for `Punctuation cleanup` with `Keep`, `Remove all`, and `Remove trailing period`.

- [ ] **Step 2: Wire load/save**

On load, set controls from `AppSettings`. On save, persist `RemoveFillerWords`, `PunctuationCleanupMode`, `LowercaseTranscription`, and `AppendTrailingSpace` with the existing model path.

- [ ] **Step 3: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 warnings and 0 errors.

- [ ] **Step 4: Commit UI**

Commit message:

```text
feat(windows): expose cleanup settings in shell
```

## Task 2: Update Docs And Verify

**Files:**

- Modify: `README.md`

- [ ] **Step 1: Update README**

Mention that the current Windows shell can configure cleanup behavior for filler words, punctuation cleanup, lowercase output, and trailing spaces.

- [ ] **Step 2: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [ ] **Step 3: Commit docs**

Commit message:

```text
docs(windows): note cleanup settings controls
```

## Plan Self-Review

Spec coverage:

- Covers the user-facing controls for the cleanup settings already added to Core and persistence.
- Leaves full macOS-style settings navigation, Dictionary UI, and onboarding to future plans.

Placeholder scan:

- No placeholders remain.

Type consistency:

- Uses existing `AppSettings` and `PunctuationCleanupMode` names.
