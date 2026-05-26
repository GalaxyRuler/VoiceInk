# Windows Model Storage Guidance Implementation Plan

**Goal:** Add scan-friendly model storage/import/backup/warmup guidance to the AI Models page.

**Tech Stack:** .NET 10 Core presenter records, WinUI ListView, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/ModelLibraryOverviewPresenterTests.cs`

Add failing assertions that `ModelLibraryOverviewPresentation` exposes storage guidance rows for:

- app-local downloaded models;
- imported file references;
- backup behavior;
- warmup behavior.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter ModelLibraryOverviewPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/ModelLibraryOverviewPresenter.cs`

Add `ModelLibraryStorageGuidanceRow` and populate stable guidance rows.

## Task 3: WinUI Rendering

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Render storage guidance below existing model action rows.

## Task 4: Docs and Verification

- Update: `docs/superpowers/project-completion.md`

Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
