# Windows Context Provider Boundary Implementation Plan

**Goal:** Make Enhancement context privacy rows explicitly reflect the selected enhancement provider boundary.

**Tech Stack:** .NET 10 Core presenter records, WinUI existing ListView binding, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementContextReadinessPresenterTests.cs`

Add failing assertions for:

- cloud provider boundary copy when a cloud enhancement provider is selected;
- local provider boundary copy when Ollama or Local CLI is selected.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter EnhancementContextReadinessPresenterTests
```

## Task 2: Presenter Logic

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextReadinessPresenter.cs`

Resolve the selected enhancement provider and add a privacy row only when Enhancement is enabled. Treat Ollama and Local CLI as local providers; all other presets disclose the cloud prompt boundary.

## Task 3: Docs and Verification

- Add this spec and plan.
- Update `docs/superpowers/project-completion.md`.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
