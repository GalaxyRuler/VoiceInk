# Windows Onboarding Text Insertion Readiness Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add testable Windows text insertion readiness guidance to onboarding as the platform adaptation of macOS Accessibility Access onboarding.

**Architecture:** Keep the behavior in `VoiceInk.Windows.Core.Onboarding.OnboardingChecklistPresenter` so the WinUI shell can bind rows without UI-only logic. Tests assert presenter output, display text, and accessible names.

**Tech Stack:** C#, xUnit, .NET 10, WinUI presenter binding.

---

### Task 1: Add Presenter Tests

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingChecklistPresenterTests.cs`

- [ ] **Step 1: Write the failing test**

Add a test named `Present_ShowsWindowsTextInsertionReadinessGuidance`:

```csharp
[Fact]
public void Present_ShowsWindowsTextInsertionReadinessGuidance()
{
    var status = OnboardingSetupStatusService.Build(
        new AppSettings
        {
            ModelPath = "C:\\Models\\ggml-base.en.bin",
            Hotkey = "Ctrl+Alt+Space"
        },
        hasAudioInputChoices: true);

    var presentation = OnboardingChecklistPresenter.Present(status);

    Assert.Contains(
        presentation.Items,
        item => item.Title == "Text insertion"
            && item.Description == "VoiceInk inserts through the focused field by using clipboard paste; History keeps the transcript available if the target app blocks paste or loses focus."
            && item.State == OnboardingChecklistItemState.Advisory);
    Assert.Contains(
        presentation.SummaryRows,
        row => row.Title == "Text Insertion"
            && row.Description == "Uses clipboard paste into the focused field; recover from History if the target app rejects insertion."
            && row.StatusBadge == "Review");
    Assert.Contains(
        presentation.SetupActions,
        action => action.Title == "Text Insertion"
            && action.Description == "Click a target field before recording. VoiceInk pastes the transcript there and keeps a History copy for recovery."
            && action.CommandText == "Review Flow"
            && action.StatusBadge == "Review");
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingChecklistPresenterTests.Present_ShowsWindowsTextInsertionReadinessGuidance -nr:false -p:UseSharedCompilation=false
```

Expected: FAIL because the presenter does not yet expose the text insertion rows.

### Task 2: Implement Presenter Rows

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingChecklistPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingChecklistPresenterTests.cs`

- [ ] **Step 1: Add the checklist, summary, and action rows**

Add:

```csharp
new OnboardingChecklistItemPresentation(
    "Text insertion",
    "VoiceInk inserts through the focused field by using clipboard paste; History keeps the transcript available if the target app blocks paste or loses focus.",
    OnboardingChecklistItemState.Advisory)
```

Add:

```csharp
new OnboardingSummaryRowPresentation(
    "Text Insertion",
    "Uses clipboard paste into the focused field; recover from History if the target app rejects insertion.",
    "Review")
```

Add:

```csharp
new OnboardingSetupActionPresentation(
    "Text Insertion",
    "Click a target field before recording. VoiceInk pastes the transcript there and keeps a History copy for recovery.",
    "Review Flow",
    "Review")
```

- [ ] **Step 2: Update existing collection-count/order expectations**

Update expected progress labels from `2 of 6` to `2 of 7`, `4 of 6` to `4 of 7`, and collection assertions to include the new row in the chosen order.

- [ ] **Step 3: Run focused tests**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingChecklistPresenterTests -nr:false -p:UseSharedCompilation=false
```

Expected: PASS.

### Task 3: Verify And Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [ ] **Step 1: Update parity docs**

Add the onboarding text insertion readiness row to the parity spec and project tracker.

- [ ] **Step 2: Run broad verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
git diff --check
```

Expected: all commands succeed; `git diff --check` may report existing CRLF notices but no whitespace errors.

- [ ] **Step 3: Commit**

Run:

```powershell
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingChecklistPresenter.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingChecklistPresenterTests.cs docs/superpowers/specs/2026-05-27-windows-onboarding-text-insertion-readiness.md docs/superpowers/plans/2026-05-27-windows-onboarding-text-insertion-readiness.md docs/superpowers/project-completion.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add onboarding insertion readiness"
```
