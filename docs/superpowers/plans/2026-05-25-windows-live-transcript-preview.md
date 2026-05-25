# Windows Live Transcript Preview Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style live transcript preview plumbing and floating recorder rendering for real partial transcript sources.

**Architecture:** Keep partial transcript state in Core via `DictationController`, keep display rules in `FloatingRecorderPresenter`, and keep WinUI limited to rendering the view state and saving the user setting. Do not fake partial transcripts; this slice prepares the UI/controller path for later streaming providers.

**Tech Stack:** .NET 10, WinUI 3, xUnit, JSON settings persistence.

---

### Task 1: Core Preview State And Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderPresenterTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderViewState.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderPresenter.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`

- [x] **Step 1: Add failing presenter tests**

Add tests proving preview text appears only while recording, enabled, and non-empty, and that whitespace is trimmed.

- [x] **Step 2: Add failing controller tests**

Add tests proving partial transcript updates are accepted only while recording and are cleared after stop/cancel.

- [x] **Step 3: Run focused tests and verify red**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~FloatingRecorderPresenterTests|FullyQualifiedName~DictationControllerTests"
```

Expected: fails because the new properties/methods do not exist yet.

- [x] **Step 4: Implement minimal Core changes**

Add `LiveTranscript` and `HasLiveTranscript` to `FloatingRecorderViewState`, add presenter parameters for `partialTranscript` and `showLiveTranscriptPreview`, and add `PartialTranscript` plus `UpdatePartialTranscript`/clear behavior to `DictationController`.

- [x] **Step 5: Re-run focused tests**

Expected: focused tests pass.

### Task 2: Settings Persistence And WinUI Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`

- [x] **Step 1: Add failing settings/backup assertions**

Update settings persistence and backup tests to include `ShowLiveTranscriptPreview = true`.

- [x] **Step 2: Run focused settings/backup tests and verify red**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~JsonSettingsStoreTests|FullyQualifiedName~VoiceInkSettingsBackupTests"
```

Expected: fails until `AppSettings` carries the new property.

- [x] **Step 3: Implement setting and UI wiring**

Add `ShowLiveTranscriptPreview` to `AppSettings`, expose `Show Live Transcript Preview` beside `Prewarm Local Model`, save it like the prewarm setting, and pass the setting plus controller partial text into the floating recorder presenter.

- [x] **Step 4: Add floating recorder live transcript panel**

Add a collapsed live transcript panel above recorder chrome, populate it from `FloatingRecorderViewState.LiveTranscript`, and resize the no-activate recorder window upward when the panel is visible.

- [x] **Step 5: Re-run focused tests and Debug build**

Expected: settings/backup tests pass and the app builds.

### Task 3: Docs, Review, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: this plan file

- [x] **Step 1: Update docs and tracker**

Document that Windows now has live transcript preview plumbing and UI, while streaming providers remain the source gap.

- [x] **Step 2: Run full verification**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

- [x] **Step 3: Request review and fix Critical/Important findings**

Review focus: no fake transcript generation, correct recording-boundary clears, setting persistence, no-activate window sizing, docs do not overstate streaming parity.

- [x] **Step 4: Commit slice**

```powershell
git add VoiceInk.Windows README.md docs\superpowers
git diff --cached --check
git commit -m "feat(windows): add live transcript recorder preview plumbing"
```
