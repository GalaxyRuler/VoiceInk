# Windows Floating Recorder Hover Dismissal Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the Windows floating-recorder Prompt and Power chooser panels dismiss like the macOS recorder popovers: stay open while the pointer is over the button or panel, then close after a short delay once both are no longer hovered.

**Architecture:** Add a small Core hover-dismissal policy object that tracks button/panel hover state and tells the UI whether to open immediately, schedule delayed dismissal, cancel delayed dismissal, or close. Wire it to WinUI pointer enter/exit events and a non-repeating `DispatcherQueueTimer` so the behavior remains inside the existing no-activate recorder window.

**Tech Stack:** .NET 10, WinUI 3 pointer events, Windows App SDK `DispatcherQueueTimer`, xUnit.

---

### Task 1: Core Hover Dismissal Policy

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recorder/FloatingRecorderPopoverHoverController.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recorder/FloatingRecorderPopoverHoverControllerTests.cs`

- [x] **Step 1: Write failing policy tests**

Cover:

- Entering the button opens immediately and cancels any delayed dismissal.
- Leaving the button while the panel is not hovered schedules delayed dismissal.
- Entering the panel after leaving the button cancels delayed dismissal and keeps the panel open.
- Leaving the panel while the button is not hovered schedules delayed dismissal.
- Timer expiry closes only when both button and panel are not hovered.
- Manual close clears hover state and delayed dismissal.

- [x] **Step 2: Run red tests**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FloatingRecorderPopoverHoverControllerTests"
```

Expected: fail because `FloatingRecorderPopoverHoverController` does not exist.

- [x] **Step 3: Implement policy**

Add:

```csharp
public enum FloatingRecorderPopoverHoverAction
{
    None,
    Open,
    ScheduleDismissal,
    CancelDismissal,
    Close
}

public sealed class FloatingRecorderPopoverHoverController
{
    public bool IsButtonHovered { get; }
    public bool IsPanelHovered { get; }
    public bool IsDismissalScheduled { get; }
    public FloatingRecorderPopoverHoverAction ButtonEntered();
    public FloatingRecorderPopoverHoverAction ButtonExited();
    public FloatingRecorderPopoverHoverAction PanelEntered();
    public FloatingRecorderPopoverHoverAction PanelExited();
    public FloatingRecorderPopoverHoverAction DismissalTimerElapsed();
    public void Reset();
}
```

- [x] **Step 4: Run green tests**

Expected: all hover controller tests pass.

### Task 2: WinUI Pointer Wiring And Timer

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/FloatingRecorderWindow.xaml.cs`

- [x] **Step 1: Add pointer event handlers**

Attach `PointerEntered` and `PointerExited` handlers to `PromptButton`, `PromptPopoverPanel`, `PowerModeButton`, and `PowerModePopoverPanel`.

- [x] **Step 2: Add hover controllers and dismissal timer**

Add one `FloatingRecorderPopoverHoverController` per popover plus a non-repeating `DispatcherQueueTimer` with `Interval = TimeSpan.FromMilliseconds(250)`.

- [x] **Step 3: Apply hover actions**

On hover action:

- `Open`: open the relevant popover.
- `CancelDismissal`: stop the timer.
- `ScheduleDismissal`: start/restart the timer for the active popover.
- `Close`: close the active popover.

Manual button click should still toggle immediately and reset hover state when closing.

- [x] **Step 4: Run focused build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 3: Docs, Review, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: this plan file

- [x] **Step 1: Update docs**

Document hover-delay Prompt/Power dismissal as complete. Keep live partial transcript and notch-style recorder as remaining floating recorder gaps.

- [x] **Step 2: Verify focused tests and build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FloatingRecorderPopoverHoverControllerTests|FloatingRecorderControlPresenterTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

- [x] **Step 3: Request review and fix Critical/Important findings**

Review focus: hover state correctness, timer lifetime, no-activate behavior, manual click behavior, and docs scope.

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
git commit -m "feat(windows): add recorder popover hover dismissal"
```
