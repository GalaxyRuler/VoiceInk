# Windows Metrics Data Guidance Implementation Plan

**Goal:** Add scan-friendly metric source/formula/export guidance to the Windows Metrics page.

**Tech Stack:** .NET 10 Core presenter records, WinUI ListView, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Metrics/SessionMetricsDashboardPresenterTests.cs`

Add failing assertions that `SessionMetricsDashboardPresentation` exposes data guidance rows for:

- sessions and words;
- words per minute;
- keystrokes/time saved;
- CSV/reset scope.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter SessionMetricsDashboardPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/SessionMetricsDashboardPresenter.cs`

Add `SessionMetricsDataGuidanceRow` and populate stable guidance rows.

## Task 3: WinUI Rendering

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Render guidance rows near the existing action/diagnostics rows.

## Task 4: Docs and Verification

- Update: `docs/superpowers/project-completion.md`

Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
