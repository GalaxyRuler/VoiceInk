# Windows Dictionary Rule Guidance Implementation Plan

**Goal:** Add scan-friendly dictionary rule guidance to the Windows Dictionary page.

**Tech Stack:** .NET 10 Core presenter records, WinUI ListView, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictionary/DictionaryPagePresenterTests.cs`

Add failing assertions that `DictionaryPagePresentation` exposes guidance rows for:

- vocabulary prompt support;
- replacement timing;
- disabled replacement behavior;
- local import/export scope.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter DictionaryPagePresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictionary/DictionaryPagePresenter.cs`

Add `DictionaryRuleGuidanceRow` and populate stable guidance rows.

## Task 3: WinUI Rendering

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Render rule guidance rows beneath dictionary summary rows.

## Task 4: Docs and Verification

- Update: `docs/superpowers/project-completion.md`

Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
