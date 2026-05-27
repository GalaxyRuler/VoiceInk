# Windows Floating Recorder Control Accessibility Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make floating recorder Prompt and Power Mode button automation names/help text match the current dynamic selections.

**Architecture:** Keep semantic strings in `FloatingRecorderControlPresenter` so WinUI binding stays thin and presenter behavior is testable without GUI automation. `FloatingRecorderWindow` applies the strings to `AutomationProperties.Name`, `AutomationProperties.HelpText`, and tooltips.

**Tech Stack:** C#, .NET 10, WinUI 3, xUnit.

---

### Task 1: Add Presenter Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderControlPresenterTests.cs`

- [ ] **Step 1: Write failing assertions**

Add assertions to the selected prompt/Power Mode tests:

```csharp
Assert.Equal("Recorder prompt: Chat", state.PromptButtonAccessibleName);
Assert.Equal("Opens the recorder prompt chooser. Current prompt: Chat.", state.PromptButtonHelpText);
Assert.Equal("Recorder Power Mode: > Terminal", state.PowerModeButtonAccessibleName);
Assert.Equal("Opens the recorder Power Mode chooser. Current selection: > Terminal.", state.PowerModeButtonHelpText);
```

Add assertions for enhancement disabled:

```csharp
Assert.Equal("Recorder prompt chooser", state.PromptButtonAccessibleName);
Assert.Equal("Opens the recorder prompt chooser and enables AI enhancement before selecting a prompt.", state.PromptButtonHelpText);
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FloatingRecorderControlPresenterTests -nr:false -p:UseSharedCompilation=false
```

Expected: FAIL because the presenter does not yet expose these fields.

### Task 2: Implement Presenter Fields

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderControlPresenter.cs`

- [ ] **Step 1: Add fields to `FloatingRecorderControlState`**

Add four string fields after the visible button fields.

- [ ] **Step 2: Populate strings**

Use the visible prompt title and Power Mode button label to construct accessible names/help text. Keep strings deterministic and culture-invariant.

- [ ] **Step 3: Run focused tests**

Run the focused test command. Expected: PASS.

### Task 3: Bind WinUI Automation Properties

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`

- [ ] **Step 1: Apply names/help text**

Set:

```csharp
AutomationProperties.SetName(PromptButton, state.PromptButtonAccessibleName);
AutomationProperties.SetHelpText(PromptButton, state.PromptButtonHelpText);
AutomationProperties.SetName(PowerModeButton, state.PowerModeButtonAccessibleName);
AutomationProperties.SetHelpText(PowerModeButton, state.PowerModeButtonHelpText);
```

- [ ] **Step 2: Keep tooltips aligned**

Use the same help text for tooltips.

- [ ] **Step 3: Build**

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

Mention dynamic recorder control UI Automation labels/help text in the floating recorder area.

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
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderControlPresenter.cs VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderControlPresenterTests.cs docs/superpowers/specs/2026-05-27-windows-floating-recorder-control-accessibility.md docs/superpowers/plans/2026-05-27-windows-floating-recorder-control-accessibility.md docs/superpowers/project-completion.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): improve recorder control accessibility"
```
