# Windows Model Health Guidance Implementation Plan

**Goal:** Replace the selected local model's single repair hint with scan-friendly, presenter-backed lifecycle rows.

**Tech Stack:** .NET 10 Core presenter records, WinUI ListView binding, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/LocalWhisperModelHealthPresenterTests.cs`

Add failing tests for:

- ready model guidance rows;
- missing or broken model repair guidance rows.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter LocalWhisperModelHealthPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/LocalWhisperModelHealthPresenter.cs`

Add `LocalWhisperModelHealthGuidanceRow` and include stable row lists in `LocalWhisperModelHealthPresentation`.

## Task 3: WinUI Rendering

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Render the selected model health rows next to the existing repair action.

## Task 4: Docs and Verification

- Add this spec and plan.
- Update `docs/superpowers/project-completion.md`.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
