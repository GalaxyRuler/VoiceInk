# Windows Onboarding Permission Summary Implementation Plan

**Goal:** Surface Windows microphone privacy guidance inside the first-run onboarding summary.

**Tech Stack:** .NET 10 Core presenter records, existing WinUI onboarding summary binding, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Onboarding/OnboardingChecklistPresenterTests.cs`

Add failing assertions for a Windows Permission summary row in:

- normal setup with microphone input visible;
- missing-microphone setup after device refresh.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter OnboardingChecklistPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Onboarding/OnboardingChecklistPresenter.cs`

Add the summary row using `Review` for visible microphones and `Check` when no input is visible.

## Task 3: Docs and Verification

- Add this spec and plan.
- Update `docs/superpowers/project-completion.md`.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
