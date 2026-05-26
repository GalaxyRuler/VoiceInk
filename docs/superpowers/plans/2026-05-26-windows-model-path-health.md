# Windows Model Path Health Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add test-covered local Whisper model path health checks and surface them in the WinUI model page.

**Architecture:** Keep health classification in Core with injected file existence/length delegates. WinUI consumes the result to update status text and button enabled states.

**Tech Stack:** .NET 10, C#, WinUI 3, xUnit.

---

### Task 1: Core Model Path Health

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/LocalWhisperModelHealth.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/LocalWhisperModelService.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/LocalWhisperModelServiceTests.cs`

- [x] Add failing tests for blank, non-bin, missing, empty, suspiciously small, and ready model paths.
- [x] Run `& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter LocalWhisperModelServiceTests`.
- [x] Implement `LocalWhisperModelHealth` and `LocalWhisperModelService.CheckPathHealth`.
- [x] Rerun the focused model tests.

### Task 2: WinUI Model Page Wiring

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] Use the Core health check when refreshing model choices and catalog actions.
- [x] Show a clear default model status for each health state.
- [x] Disable manual warmup, Set as Default, and Show in Explorer unless the selected/downloaded model path is ready.
- [x] Run a Debug x64 build.

### Task 3: Docs, Verification, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [x] Document model path health checks.
- [x] Run full solution tests.
- [x] Run Debug x64 build.
- [x] Run `git diff --check`.
- [x] Review and commit with `feat(windows): add model path health checks`.
