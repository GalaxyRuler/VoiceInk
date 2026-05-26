# Windows History Analysis Storage Implementation Plan

**Goal:** Make provider timing and audio storage availability visible in the inline History analysis panel.

**Tech Stack:** .NET 10 Core presenter records, existing WinUI ListView binding, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryAnalysisPresenterTests.cs`

Add failing assertions for:

- provider/timing row;
- text-only storage row;
- audio-saved storage row.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter HistoryAnalysisPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryAnalysisPresenter.cs`

Append Provider and Audio Storage rows using existing history item metadata.

## Task 3: Docs and Verification

- Add this spec and plan.
- Update `docs/superpowers/project-completion.md`.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
