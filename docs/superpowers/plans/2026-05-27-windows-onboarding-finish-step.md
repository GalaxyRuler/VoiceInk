# Windows Onboarding Finish Step Implementation Plan

**Goal:** Add a final first-run verification/tutorial step to onboarding.

**Tech Stack:** .NET 10 Core presenter, existing WinUI onboarding ListView, xUnit.

## Task 1: Failing Test

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingChecklistPresenterTests.cs`

Expect a fifth tutorial step:

- title: `Check insertion and History`;
- description: verify inserted text and use History for review/retry/recovery;
- status follows the existing tutorial readiness state.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingChecklistPresenterTests
```

## Task 2: Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingChecklistPresenter.cs`

Append the fifth tutorial row in `TutorialSteps`.

## Task 3: Docs and Verification

- Update: `docs/superpowers/project-completion.md`

Run focused onboarding tests, full solution tests, Debug x64 build, and `git diff --check`.
