# Windows Floating Recorder Prompt And Power Controls Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Windows floating recorder Prompt and Power buttons functional while preserving the dictation paste target.

**Architecture:** Add a small Core presenter for recorder prompt/power choices so labels, selected states, and enabled states are testable outside WinUI. Persist prompt selection through existing enhancement settings. Add an explicit selected Power Mode rule id to settings and teach `PowerModeMatcher` to prefer it, while `DictationController` keeps the recording-start target but reloads current settings at stop/cancel so recorder changes made during capture affect the active transcription.

**Tech Stack:** .NET 10, WinUI 3, xUnit, existing JSON settings and Core Power Mode/enhancement models.

---

### Task 1: Core Recorder Prompt/Power Choice Model

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderControlPresenter.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderControlPresenterTests.cs`

- [x] **Step 1: Write failing presenter tests**

Cover:

- Prompt choices resolve the selected prompt id through `EnhancementPromptLibrary.ResolveSelectedPromptId`.
- Prompt button remains enabled when enhancement is disabled, because the macOS button enables enhancement on first click.
- Power Mode choices include an `Automatic` item plus enabled rules only.
- Power Mode button is disabled when there are no enabled rules.
- Explicit selected Power Mode id selects that rule; missing/deleted ids fall back to `Automatic`.

- [x] **Step 2: Run red tests**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FloatingRecorderControlPresenterTests"
```

Expected: fail because `FloatingRecorderControlPresenter` does not exist.

- [x] **Step 3: Implement presenter records and builder**

Add records for:

- `FloatingRecorderPromptChoice(Guid Id, string Title, bool IsSelected, bool IsDisabled)`
- `FloatingRecorderPowerModeChoice(Guid? Id, string Title, string Emoji, bool IsSelected)`
- `FloatingRecorderControlState(string PromptTitle, bool IsEnhancementEnabled, bool CanOpenPromptControls, IReadOnlyList<FloatingRecorderPromptChoice> PromptChoices, string PowerModeTitle, string PowerModeEmoji, bool CanOpenPowerModeControls, IReadOnlyList<FloatingRecorderPowerModeChoice> PowerModeChoices)`

Add:

```csharp
public static FloatingRecorderControlState FromSettings(
    AppSettings settings,
    IReadOnlyList<EnhancementPrompt> prompts,
    IReadOnlyList<PowerModeRule> powerModeRules)
```

- [x] **Step 4: Run green tests**

Expected: all `FloatingRecorderControlPresenterTests` pass.

### Task 2: Explicit Recorder Power Mode Selection And Current-Recording Settings

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeMatcher.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/PowerMode/PowerModeMatcherTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`

- [x] **Step 1: Write failing matcher and controller tests**

Add tests that:

- `PowerModeMatcher.Resolve` prefers `AppSettings.SelectedPowerModeRuleId` over active-window/default rules when it points at an enabled rule.
- Missing, disabled, or deleted selected ids fall back to the existing target/default behavior.
- `DictationController.StopAsync` uses the recording-start target but reloads latest settings at stop, allowing a prompt or explicit Power Mode selected during recording to affect the active result.

- [x] **Step 2: Run red tests**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "PowerModeMatcherTests|DictationControllerTests|VoiceInkSettingsBackupTests"
```

Expected: fail because `SelectedPowerModeRuleId` and latest-settings-at-stop behavior do not exist.

- [x] **Step 3: Implement settings and matcher**

Add `Guid? SelectedPowerModeRuleId` to `AppSettings`, equality, and hashing. In `PowerModeMatcher.Resolve`, select the enabled rule with that id before target/default matching. Keep fallback behavior unchanged when no enabled selected rule exists.

- [x] **Step 4: Implement current-recording settings reload**

Change `DictationController.StopAsync` and `CancelAsync` to reload current settings after capture stops, then call `PowerModeMatcher.Resolve(currentSettings, activePowerModeResolution?.Target)`. This preserves the recording-start target while honoring recorder Prompt/Power changes made during recording.

- [x] **Step 5: Run green tests**

Expected: focused matcher/controller/backup tests pass.

### Task 3: WinUI Floating Recorder Controls

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Replace disabled placeholders with enabled buttons**

Prompt button should show the current prompt title, and Power button should show the active explicit Power Mode emoji/name or `Auto`. Keep buttons compact and non-activating.

- [x] **Step 2: Add control callbacks**

Expose callbacks from `FloatingRecorderWindow`:

- `PromptRequested`
- `PowerModeRequested`

In the first Windows slice, use no-activate click behavior to cycle safely without opening a focus-stealing popup:

- Prompt click: if enhancement is disabled, enable enhancement and persist. If enabled, select the next prompt.
- Power click: cycle `Automatic -> enabled rule 1 -> enabled rule 2 -> Automatic`.

Update the recorder labels and tooltips after each change.

- [x] **Step 3: Wire callbacks in `MainWindow`**

Use `FloatingRecorderControlPresenter.FromSettings` from current UI/settings state. On Prompt click, update `EnhancementEnabledCheckBox` and `EnhancementPromptComboBox`, save settings, and refresh UI. On Power click, update `SelectedPowerModeRuleId`, save settings, and refresh UI.

- [x] **Step 4: Keep active-window detection stable**

Do not call `Activate()`. Keep the existing no-activate subclass. Ensure the floating recorder window handle remains excluded from `ActiveWindowPowerModeTargetProvider`.

### Task 4: Docs, Review, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: this plan file

- [x] **Step 1: Update docs**

Document functional recorder Prompt and Power controls. Keep richer hover popovers, current active Power Mode visual parity, browser URL rules, auto-send keys, and notch style as later gaps.

- [x] **Step 2: Verify focused tests and build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FloatingRecorderControlPresenterTests|PowerModeMatcherTests|DictationControllerTests|VoiceInkSettingsBackupTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

- [x] **Step 3: Request review and fix Critical/Important findings**

Review focus: current-recording settings semantics, Power Mode selected-id persistence, no focus theft, and docs scope.

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
git commit -m "feat(windows): enable recorder prompt and power controls"
```
