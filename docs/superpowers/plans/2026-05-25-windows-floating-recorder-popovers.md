# Windows Floating Recorder Popovers Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the Windows floating recorder Prompt/Power cycle-only controls with no-activate in-window chooser popovers that match the macOS recorder behavior more closely.

**Architecture:** Keep chooser state in Core presenter records so labels, headers, empty states, selected rows, and disabled rows are testable without WinUI. Host the chooser panels inside `FloatingRecorderWindow` instead of WinUI `Flyout`, resizing the existing no-activate window upward while a chooser is open. Persist selections through the existing `MainWindow` settings path and `FloatingRecorderControlUpdateCoordinator` so Stop/Cancel waits for in-flight changes.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK `AppWindow.Show(false)`, Win32 `WM_MOUSEACTIVATE` no-activation subclass, xUnit.

---

### Task 1: Core Popover Labels And Choice State

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderControlPresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderControlPresenterTests.cs`

- [x] **Step 1: Write failing presenter tests**

Add tests that verify:

- Prompt state exposes `PromptHeaderTitle = "AI Enhancement"` and `CanToggleEnhancement = true`.
- Power state exposes `PowerModeHeaderTitle = "Select Power Mode"` and `PowerModeEmptyTitle = "No Power Modes Available"`.
- Power choices always include `Auto` as the first selectable item.
- Prompt choices remain present but disabled when enhancement is off.

- [x] **Step 2: Run red tests**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FloatingRecorderControlPresenterTests"
```

Expected: fail because the new popover header/empty-state properties do not exist.

- [x] **Step 3: Extend presenter records**

Extend `FloatingRecorderControlState` with:

```csharp
string PromptHeaderTitle,
bool CanToggleEnhancement,
string PowerModeHeaderTitle,
string PowerModeEmptyTitle
```

Populate them from `FromSettings` with macOS-aligned labels:

```csharp
"AI Enhancement"
true
"Select Power Mode"
"No Power Modes Available"
```

- [x] **Step 4: Run green tests**

Expected: all `FloatingRecorderControlPresenterTests` pass.

### Task 2: In-Window Popover Host

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`

- [x] **Step 1: Add no-activate popover XAML**

Add a row above `RecorderChrome` with two hidden panels:

- Prompt panel width near `220`, black background, header `CheckBox`, divider, scrollable prompt row stack.
- Power panel width near `220`, black background, header text, divider, scrollable Power Mode row stack and empty text.
- Keep all controls inside the existing `Window` root, not a `Flyout`, `Popup`, `ContentDialog`, or secondary window.

- [x] **Step 2: Add chooser callbacks**

Replace cycle-only callbacks with:

```csharp
public Func<bool, Task>? PromptEnhancementToggled { get; set; }
public Func<Guid, Task>? PromptChoiceRequested { get; set; }
public Func<Guid?, Task>? PowerModeChoiceRequested { get; set; }
```

Keep `PromptRequested` and `PowerModeRequested` only if they still mean "toggle chooser", not "cycle next choice".

- [x] **Step 3: Render rows in code-behind**

In `ApplyControls`, cache the latest `FloatingRecorderControlState`, set button labels/tooltips, update the enhancement checkbox, render prompt/power rows, and hide any open chooser when controls become unusable.

Each prompt row should show:

```text
<prompt title>    checkmark if selected
```

Each Power Mode row should show:

```text
<emoji> <name>    checkmark if selected
```

- [x] **Step 4: Resize upward while a chooser is open**

Add constants:

```csharp
private const int RecorderWindowWidth = 384;
private const int RecorderWindowCollapsedHeight = 104;
private const int RecorderWindowExpandedHeight = 352;
```

When a chooser opens, call `AppWindow.Resize(new SizeInt32(RecorderWindowWidth, RecorderWindowExpandedHeight))` and `MoveBottomCenter()`. When it closes, resize back to `RecorderWindowCollapsedHeight`. This keeps the recorder bottom anchored above the taskbar.

- [x] **Step 5: Run build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 3: Main Window Selection Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Replace cycle callbacks**

Wire `EnsureFloatingRecorderWindow` so:

- Prompt button toggles the prompt chooser in `FloatingRecorderWindow`.
- Power button toggles the Power Mode chooser in `FloatingRecorderWindow`.
- Prompt enhancement toggle calls a new `SetFloatingRecorderEnhancementAsync(bool isEnabled)`.
- Prompt row selection calls a new `SelectFloatingRecorderPromptAsync(Guid promptId)`.
- Power row selection calls a new `SelectFloatingRecorderPowerModeAsync(Guid? ruleId)`.

- [x] **Step 2: Persist Prompt choices through existing settings**

`SetFloatingRecorderEnhancementAsync` sets `EnhancementEnabledCheckBox.IsChecked`, then calls `SaveFloatingRecorderControlSettingsAsync("AI enhancement enabled")` or `SaveFloatingRecorderControlSettingsAsync("AI enhancement disabled")`.

`SelectFloatingRecorderPromptAsync` enables enhancement, selects the prompt through `SelectEnhancementPrompt(promptId)`, then calls `SaveFloatingRecorderControlSettingsAsync($"Prompt: {title}")`.

- [x] **Step 3: Persist Power Mode choices through existing settings**

`SelectFloatingRecorderPowerModeAsync` sets `selectedPowerModeRuleId` to the selected rule id or `null` for Auto, then calls `SaveFloatingRecorderControlSettingsAsync("Power Mode: Auto")` or `SaveFloatingRecorderControlSettingsAsync($"Power Mode: {title}")`.

- [x] **Step 4: Preserve Stop/Cancel race protection**

Keep `SaveFloatingRecorderControlSettingsAsync` using `floatingRecorderControlUpdates.RunUpdateAsync(...)`, and keep Stop/Cancel waiting on `floatingRecorderControlUpdates.WaitForPendingUpdateAsync(...)`.

- [x] **Step 5: Run focused build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 4: Docs, Review, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: this plan file

- [x] **Step 1: Update docs**

Document that Prompt and Power controls now open no-activate in-window chooser panels. Keep hover-delay dismissal, live partial transcript, notch style, browser URL matching, auto-send keys, and Power Mode shortcuts as remaining gaps.

- [x] **Step 2: Verify focused tests and build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FloatingRecorderControlPresenterTests|FloatingRecorderControlUpdateCoordinatorTests|DictationControllerTests|PowerModeMatcherTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

- [x] **Step 3: Request review and fix Critical/Important findings**

Review focus: no activation/focus theft, chooser persistence before Stop/Cancel, Power Mode `Auto` behavior, prompt enhancement toggle behavior, and docs scope.

- [x] **Step 4: Full verification**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
git diff --cached --check
```

- [x] **Step 5: Commit**

```powershell
git add VoiceInk.Windows README.md docs\superpowers
git commit -m "feat(windows): add recorder prompt and power popovers"
```
