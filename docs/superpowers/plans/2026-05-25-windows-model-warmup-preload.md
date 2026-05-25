# Windows Model Warmup Preload Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-aligned local model warmup/preload for the Windows fork so the selected local Whisper model is validated and primed after startup, after wake, after download/default selection, and on manual request.

**Architecture:** Core owns a testable warmup coordinator, state, and policy so WinUI can schedule warmups without blocking recording or binding directly to Whisper.net. Native owns the Whisper.net warmup adapter and validates/loads the selected GGML model by creating a factory and processor, then disposing them. WinUI owns startup/manual/resume triggers, UI status, and settings persistence.

**Tech Stack:** .NET 10, WinUI 3, Whisper.net 1.9.0, `Microsoft.Win32.SystemEvents.PowerModeChanged`, xUnit.

---

### Task 1: Core Warmup Policy And State

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/IWhisperModelWarmupService.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/WhisperModelWarmupState.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Models/WhisperModelWarmupCoordinator.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Models/WhisperModelWarmupCoordinatorTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`

- [x] **Step 1: Write failing tests**

Add tests proving:

- `PrewarmModelOnWake` defaults to `true` and round-trips through JSON settings.
- Warmup is skipped when disabled.
- Warmup is skipped for cloud transcription providers.
- Warmup is skipped when no model path is configured or the file probe reports the model missing.
- Starting a warmup while the same model is already warming does not start duplicate work.
- Local `.en` model warmup uses the same language fallback as real transcription.
- Completion and failure update a UI-safe state snapshot.

- [x] **Step 2: Verify the tests fail**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "WhisperModelWarmupCoordinatorTests|JsonSettingsStoreTests|VoiceInkSettingsBackupTests"
```

Expected: compile/test failure because warmup types and `PrewarmModelOnWake` do not exist yet.

- [x] **Step 3: Implement minimal Core behavior**

Add:

- `IWhisperModelWarmupService.WarmupAsync(TranscriptionOptions options, CancellationToken cancellationToken)`.
- `WhisperModelWarmupStatus` enum with `Idle`, `Skipped`, `Warming`, `Succeeded`, `Failed`, `Canceled`.
- `WhisperModelWarmupState` record containing status, model path/display name, trigger, message, started/completed timestamps, and duration.
- `WhisperModelWarmupCoordinator` that accepts a warmup service, a model-exists probe, and a clock delegate. It starts nonblocking warmups, prevents duplicates, normalizes options through `TranscriptionConfiguration.BuildOptions`, updates state, and exposes an event for UI refresh.
- `AppSettings.PrewarmModelOnWake` defaulting to `true`, with equality/hash coverage.

- [x] **Step 4: Verify Core tests pass**

Run the same filtered command. Expected: all filtered tests pass.

### Task 2: Native Whisper.net Warmup Adapter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Transcription/WhisperNetModelWarmupService.cs`
- Test/build verification: native project build through the solution.

- [x] **Step 1: Implement adapter**

`WhisperNetModelWarmupService` should:

- Validate the model file exists.
- Create `WhisperFactory.FromPath(options.ModelPath)`.
- Configure language detection for `auto`, otherwise `WithLanguage(options.Language)`.
- Apply a prompt if supplied.
- Build and dispose a processor.
- Respect cancellation before and after model file validation.

- [x] **Step 2: Build native/app wiring target**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

### Task 3: WinUI Startup, Wake, Manual, And Download Triggers

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Add UI controls**

In AI Models add:

- `PrewarmModelOnWakeCheckBox` with content `Prewarm Local Model`.
- `WarmupSelectedModelButton` with content `Warm Up Selected Model`.
- `ModelWarmupStatusTextBlock` under the existing model download status.

- [x] **Step 2: Add wiring**

In `MainWindow.xaml.cs`:

- Add a `WhisperModelWarmupCoordinator` field using `WhisperNetModelWarmupService`.
- Add event handling that updates the warmup status text on the dispatcher.
- Add manual warmup handler.
- Schedule warmup after settings load, after default model changes, and after successful catalog download/default selection.
- Subscribe to `SystemEvents.PowerModeChanged`; when `PowerModes.Resume` fires and the setting is enabled, schedule warmup.
- Detach the static event and cancel active warmup on close.
- Include `PrewarmModelOnWake` in `CurrentSettingsAsync` and `ApplySettingsToUiAsync`.

- [x] **Step 3: Update docs and completion bar**

Document that Windows prewarms the selected local Whisper model by loading a Whisper.net factory/processor on startup, manual action, successful local-model selection/download, and Windows resume. Note that it is a conservative preload and not yet a persistent model cache shared with transcription.

- [x] **Step 4: Verify, review, and commit**

Run focused tests, full tests, and Debug x64 build. Request code review for the slice and fix Critical/Important findings. Commit as:

```powershell
git add VoiceInk.Windows README.md docs\superpowers
git commit -m "feat(windows): add local model warmup preload"
```
