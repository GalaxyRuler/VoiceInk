# Windows Audio Priority Fallback Row Implementation Plan

**Goal:** Make prioritized microphone fallback to System Default visible in the audio device health list.

**Tech Stack:** .NET 10 Core presenter records, existing WinUI health list, xUnit.

## Task 1: Presenter Test

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Audio/AudioInputDeviceSelectionTests.cs`

Add a failing test for prioritized mode where all configured priority devices are unavailable and the selected choice is System Default.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioInputDeviceSelectionTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceHealthPresenter.cs`

Append a selected `System Default Fallback` warning row only when prioritized mode has no available priority rows and selection falls back to System Default.

## Task 3: Docs and Verification

- Add this spec and plan.
- Update `docs/superpowers/project-completion.md`.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
