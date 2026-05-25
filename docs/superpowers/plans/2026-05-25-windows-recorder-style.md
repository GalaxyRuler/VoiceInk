# Windows Recorder Style Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style recorder style selection and a Windows top-center notch recorder adaptation.

**Architecture:** Keep the persisted setting in Core `AppSettings`, keep normalization in a small Core recorder helper, add style to `FloatingRecorderViewState`, and let WinUI render mini versus notch layout from that view state. Use the existing floating recorder window and no-activate plumbing rather than adding a second window manager.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK `AppWindow`, xUnit, JSON settings persistence.

---

### Task 1: Core Recorder Style State

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/RecorderStyleSettings.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderViewState.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderPresenterTests.cs`

- [x] **Step 1: Add failing recorder style tests**

Add tests showing that `mini` is the default, `notch` is preserved, blank/unknown values normalize to `mini`, and presenter state carries the normalized value.

- [x] **Step 2: Run focused tests and verify red**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~FloatingRecorderPresenterTests"
```

Expected: fails because the style helper/state does not exist yet.

- [x] **Step 3: Implement Core style plumbing**

Add `RecorderStyleSettings` constants and `Normalize`, add `RecorderStyle` to `FloatingRecorderViewState`, and pass an optional `recorderStyle` parameter through `FloatingRecorderPresenter.FromState`.

- [x] **Step 4: Re-run focused tests**

Expected: recorder presenter tests pass.

### Task 2: Settings Persistence And Backup

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`

- [x] **Step 1: Add failing settings/backup assertions**

Assert `RecorderStyle = "notch"` persists through JSON settings and General Settings backup/merge.

- [x] **Step 2: Run focused settings tests and verify red**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~JsonSettingsStoreTests|FullyQualifiedName~VoiceInkSettingsBackupTests"
```

Expected: fails until `AppSettings` carries the new property.

- [x] **Step 3: Implement `AppSettings.RecorderStyle`**

Default to `RecorderStyleSettings.Mini`, include it in equality/hash, and rely on existing structured JSON/backup serialization.

- [x] **Step 4: Re-run focused settings tests**

Expected: JSON settings and backup tests pass.

### Task 3: WinUI Settings And Recorder Layout

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`

- [x] **Step 1: Add Settings recorder style control**

Add a `Recorder Style` combo box with `Mini` and `Notch` to the Settings area, load it from `AppSettings`, save it through `CurrentSettingsAsync`, and disable it while recording or other operations are active.

- [x] **Step 2: Pass style into presenter/window**

Normalize the selected UI value and pass it to `FloatingRecorderPresenter.FromState`.

- [x] **Step 3: Render notch layout**

When view state style is `notch`, switch the recorder chrome to a top-style black pill, resize narrower/shorter than mini, move the window top-center, and place popovers/live text below the chrome. Preserve mini layout for `mini`.

- [x] **Step 4: Run Debug build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: app builds with no errors.

### Task 4: Docs, Review, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: this plan file

- [x] **Step 1: Update docs and completion tracker**

Document recorder style selection and notch-style Windows adaptation. Keep streaming partial transcript providers listed as a remaining floating-recorder gap.

- [x] **Step 2: Run full verification**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

- [x] **Step 3: Request code review and fix Critical/Important findings**

Review focus: macOS behavior fidelity, normalized settings, no-activate recorder behavior, top/bottom placement, docs not overstating parity, and no commercial surfaces.

- [ ] **Step 4: Commit slice**

```powershell
git add VoiceInk.Windows README.md docs\superpowers
git diff --cached --check
git commit -m "feat(windows): add recorder style selection"
```
