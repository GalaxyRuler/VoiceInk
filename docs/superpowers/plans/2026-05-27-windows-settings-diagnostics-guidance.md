# Windows Settings Diagnostics Guidance Implementation Plan

**Goal:** Make diagnostics behavior visible in Settings/About without changing logging behavior.

**Tech Stack:** .NET 10 Core presenter records, WinUI ListView binding, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Settings/SettingsSectionPresenterTests.cs`

Add failing assertions for diagnostic log export, sanitized summary copy, and Windows OS app-diagnostics privacy boundaries.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SettingsSectionPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/SettingsSectionPresenter.cs`

Add `SettingsDiagnosticsGuidanceRow` and stable row output in `SettingsSectionPresentation`.

## Task 3: WinUI Rendering

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Bind the diagnostics guidance rows into the About/Diagnostics surface near the existing diagnostics buttons.

## Task 4: Docs and Verification

- Add this spec and plan.
- Update `docs/superpowers/project-completion.md`.
- Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
