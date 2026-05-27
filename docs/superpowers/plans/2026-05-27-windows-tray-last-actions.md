# Windows Tray Last Actions Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS menu-bar parity rows for paste-last and retry-last actions to the Windows notification-area tray menu.

**Architecture:** Extend the UI-independent `TrayShellState` with testable enablement booleans. Render those booleans in `TrayIconService` and wire new events in `MainWindow.xaml.cs` to existing paste/retry methods.

**Tech Stack:** C#, .NET 10, WinForms `NotifyIcon`/`ToolStripMenuItem`, WinUI host wiring, xUnit.

---

### Task 1: Add Core Tray State Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shell/TrayShellPresenterTests.cs`

- [ ] **Step 1: Write failing assertions**

Add assertions to existing tests:

```csharp
Assert.True(state.CanPasteLastTranscription);
Assert.True(state.CanPasteLastEnhancedTranscription);
Assert.True(state.CanRetryLastTranscription);
```

For recording state:

```csharp
Assert.False(state.CanPasteLastTranscription);
Assert.False(state.CanPasteLastEnhancedTranscription);
Assert.False(state.CanRetryLastTranscription);
```

For transcribing state:

```csharp
Assert.False(state.CanRetryLastTranscription);
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter TrayShellPresenterTests -nr:false -p:UseSharedCompilation=false
```

Expected: FAIL because the tray state does not yet expose last-action booleans.

### Task 2: Extend Core Tray State

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayShellState.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayShellPresenter.cs`

- [ ] **Step 1: Add state properties**

Add `CanPasteLastTranscription`, `CanPasteLastEnhancedTranscription`, and `CanRetryLastTranscription` to `TrayShellState`.

- [ ] **Step 2: Populate properties**

Use:

```csharp
var canPasteLast = settingsLoaded && !operationActive && dictationState != DictationState.Recording;
var canRetryLast = canUseOperationalCommands && dictationState == DictationState.Idle;
```

- [ ] **Step 3: Run focused tests**

Run the same `TrayShellPresenterTests` command. Expected: PASS.

### Task 3: Add Native Tray Rows And Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Tray/TrayIconService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [ ] **Step 1: Add tray items/events**

Create menu items for paste last, paste last enhanced, and retry last. Add them near History/Quick Add, matching macOS menu action grouping.

- [ ] **Step 2: Update state application**

Set each new item `Enabled` from the matching `TrayShellState` property.

- [ ] **Step 3: Wire MainWindow events**

Subscribe/unsubscribe the events and route handlers to:

```csharp
await PasteLastAsync(LastTranscriptionTextKind.Final);
await PasteLastAsync(LastTranscriptionTextKind.EnhancedPreferred);
await RetryLastHistoryAsync();
```

- [ ] **Step 4: Run build**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
```

Expected: build succeeds.

### Task 4: Verify, Document, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [ ] **Step 1: Update parity docs**

Mention the tray paste-last and retry-last rows in Shell/tray.

- [ ] **Step 2: Run full verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
git diff --check
```

Expected: all commands succeed; `git diff --check` may show CRLF notices only.

- [ ] **Step 3: Commit**

Run:

```powershell
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayShellState.cs VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/TrayShellPresenter.cs VoiceInk.Windows/src/VoiceInk.Windows.Native/Tray/TrayIconService.cs VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shell/TrayShellPresenterTests.cs docs/superpowers/specs/2026-05-27-windows-tray-last-actions.md docs/superpowers/plans/2026-05-27-windows-tray-last-actions.md docs/superpowers/project-completion.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add tray last actions"
```
