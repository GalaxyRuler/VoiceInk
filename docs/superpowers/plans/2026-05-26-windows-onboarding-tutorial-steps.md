# Windows Onboarding Tutorial Steps Implementation Plan

**Goal:** Add macOS-style try-it-out tutorial steps to the Windows first-run onboarding dialog.

**Tech Stack:** .NET 10 Core presenter records, WinUI dialog ListView, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingChecklistPresenterTests.cs`

Add failing assertions that:

- incomplete setup returns four waiting tutorial steps;
- complete setup includes the configured primary shortcut in the press/stop steps and marks rows ready.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingChecklistPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingChecklistPresenter.cs`

Add `OnboardingTutorialStepPresentation` and `TutorialSteps` to `OnboardingChecklistPresentation`.

## Task 3: First-Run Dialog Rendering

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Add a tutorial ListView and refresh it from `presentation.TutorialSteps`.

## Task 4: Docs and Verification

- Update: `docs/superpowers/project-completion.md`

Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
