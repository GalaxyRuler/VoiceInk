# Windows Settings Backup Guidance Implementation Plan

**Goal:** Add scan-friendly backup/import category guidance to the Windows Settings page.

**Tech Stack:** .NET 10 Core presenter records, WinUI ListView, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Settings/SettingsSectionPresenterTests.cs`

Add failing assertions that `SettingsSectionPresentation` exposes backup guidance rows for:

- settings and prompts;
- provider keys;
- model references;
- dictionary data.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SettingsSectionPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/SettingsSectionPresenter.cs`

Add a backup guidance row record and populate read-only rows from existing backup behavior.

## Task 3: WinUI Rendering

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Render backup guidance rows beneath the settings preference summary.

## Task 4: Docs and Verification

- Update: `docs/superpowers/project-completion.md`

Run focused settings tests, full solution tests, Debug x64 build, and `git diff --check`.
