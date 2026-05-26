# Windows Context Privacy Boundaries Implementation Plan

**Goal:** Add scan-friendly privacy boundary rows to the Enhancement context settings.

**Tech Stack:** .NET 10 Core presenter records, WinUI ListView, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementContextReadinessPresenterTests.cs`

Add failing assertions that the context presentation exposes privacy boundary rows for:

- local-only capture timing;
- transient selection and clipboard handling;
- OCR local capture and cloud prompt inclusion boundary;
- disabled toggles preventing source requests.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter EnhancementContextReadinessPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextReadinessPresenter.cs`

Add `EnhancementContextPrivacyRow` and populate stable privacy boundary rows.

## Task 3: WinUI Rendering

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Render privacy boundary rows between source readiness and action rows.

## Task 4: Docs and Verification

- Update: `docs/superpowers/project-completion.md`

Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
