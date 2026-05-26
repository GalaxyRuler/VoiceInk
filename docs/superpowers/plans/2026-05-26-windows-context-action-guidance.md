# Windows Context Action Guidance Implementation Plan

**Goal:** Add testable action guidance rows to the Enhancement context readiness presenter and render them in the WinUI Enhancement page.

**Tech Stack:** .NET 10, Core presenter records, WinUI ListView templates, xUnit.

## Task 1: Presenter Tests

- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementContextReadinessPresenterTests.cs`

Add failing tests that:

- default settings show action rows for enabling Enhancement, enabling Clipboard Context, and source order;
- constrained OCR with missing region shows a Select Region action;
- enabled Enhancement/Clipboard/valid OCR region rows show ready state instead of setup prompts.

Verification:

```powershell
& "..\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter EnhancementContextReadinessPresenterTests
```

## Task 2: Core Presenter

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextReadinessPresenter.cs`

Add `EnhancementContextActionRow` and `ActionRows` to the presentation. Generate rows from `AppSettings` without adding platform dependencies.

## Task 3: WinUI Rendering

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Add a compact ListView for action rows under the existing context readiness list and bind it in `ApplyEnhancementContextReadinessPresentation`.

## Task 4: Docs and Verification

- Update: `docs/superpowers/project-completion.md`

Run focused tests, full solution tests, Debug x64 build, and `git diff --check`.
