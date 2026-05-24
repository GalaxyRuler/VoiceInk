# Windows Transcribe Audio MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a source-runnable Windows Transcribe Audio workflow that mirrors the macOS queued file transcription flow using local Whisper and existing history persistence.

**Architecture:** Core owns testable queue and file-transcription orchestration. Native owns Windows media import/conversion to app-owned WAV files behind an interface. WinUI owns the navigation section, picker, queue controls, and rendering of in-memory queue state.

**Tech Stack:** .NET 10, WinUI 3, Windows App SDK `FileOpenPicker.PickMultipleFilesAsync`, NAudio `MediaFoundationReader`, Whisper.net, SQLite history, xUnit.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileQueueStatus.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileQueueItem.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileQueueUpdate.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileQueueService.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/IAudioFileImportService.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileTranscriptionResult.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileTranscriptionService.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/MediaFoundationAudioFileImportService.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/AudioFiles/AudioFileQueueServiceTests.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/AudioFiles/AudioFileTranscriptionServiceTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/ShellNavigationPresenter.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shell/ShellNavigationPresenterTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `README.md`.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Update parity spec**

Record the macOS source behavior from:

- `VoiceInk/Views/AudioTranscribeView.swift`
- `VoiceInk/Views/AudioFileRow.swift`
- `VoiceInk/Models/AudioFileQueueItem.swift`
- `VoiceInk/Services/AudioFileTranscriptionManager.swift`

Windows adaptation for this slice:

- Add Transcribe Audio navigation after Dashboard.
- Use a Windows App SDK multi-file picker.
- Keep the queue in memory for the first slice.
- Convert/import supported files to app-owned WAV recordings with NAudio/Media Foundation.
- Process pending files sequentially with local Whisper, dictionary prompt biasing, cleanup settings, and SQLite history save.
- Leave drag/drop, optional enhancement, per-file save/copy, persistent queue restoration, and richer batch actions for later.

- [ ] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-transcribe-audio-mvp.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan transcribe audio"
```

Expected: docs-only commit.

## Task 2: Core Queue Red/Green

- [ ] **Step 1: Add failing queue tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/AudioFiles/AudioFileQueueServiceTests.cs` covering:

- Adds existing supported files with file names and deterministic IDs.
- Skips unsupported files and missing files.
- Skips duplicates already pending or processing.
- Removes pending items only.
- Retries failed items by returning them to pending.
- Clears the queue.

- [ ] **Step 2: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~AudioFileQueueServiceTests
```

Expected: compile failure because `VoiceInk.Windows.Core.AudioFiles` types do not exist yet.

- [ ] **Step 3: Add queue implementation**

Create queue records and `AudioFileQueueService`:

- Supported extensions: `.wav`, `.mp3`, `.m4a`, `.mp4`, `.mov`, `.aac`, `.wma`, `.wmv`, `.avi`, `.3gp`, `.3g2`, `.flac`.
- `AddFiles` normalizes full paths, checks existence, filters unsupported extensions, skips duplicate non-terminal paths, and returns `AudioFileQueueUpdate`.
- `RemovePending`, `RetryFailed`, and `Clear` return new arrays.

- [ ] **Step 4: Verify queue tests**

Run the focused queue test command. Expected: queue tests pass.

## Task 3: Core File Transcription Red/Green

- [ ] **Step 1: Add failing transcription service tests**

Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/AudioFiles/AudioFileTranscriptionServiceTests.cs` covering:

- Missing model path returns failure before import.
- Successful file transcription imports the source file, transcribes, applies cleanup/dictionary replacements, saves a completed history row, and returns the saved item.
- Empty final text returns failure without saving history.
- Import/transcription exceptions return failed results.

- [ ] **Step 2: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~AudioFileTranscriptionServiceTests
```

Expected: compile failure because the transcription service types do not exist yet.

- [ ] **Step 3: Add core transcription implementation**

Create:

- `IAudioFileImportService.PrepareAsync(string sourcePath, string recordingsDirectory, CancellationToken cancellationToken)`.
- `AudioFileTranscriptionResult`.
- `AudioFileTranscriptionService.TranscribeAsync(string sourcePath, string recordingsDirectory, CancellationToken cancellationToken)`.

Implementation details:

- Load settings and require `ModelPath`.
- Load dictionary vocabulary/replacements.
- Render vocabulary prompt with `DictionaryService.RenderVocabularyPrompt`.
- Import/convert source file to app-owned audio through `IAudioFileImportService`.
- Transcribe with `ITranscriptionService`.
- Run `TextPostProcessor.Process`.
- Save a completed `TranscriptionHistoryItem` using imported audio metadata/path.
- Return a failure result for validation and non-cancellation exceptions.

- [ ] **Step 4: Verify transcription service tests**

Run the focused transcription service test command. Expected: tests pass.

## Task 4: Native Media Import

- [ ] **Step 1: Add NAudio import service**

Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/MediaFoundationAudioFileImportService.cs`.

Implementation details:

- Use `MediaFoundationReader` to read supported audio/video files through Windows codecs.
- Write an app-owned WAV file under the existing recordings directory with a `transcribed_<guid>.wav` name.
- Return `AudioCaptureResult` with destination path, source duration, sample rate, and channel count.
- Throw a clear `InvalidOperationException` when Media Foundation cannot decode the source file.

- [ ] **Step 2: Verify app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings/errors.

## Task 5: Navigation And WinUI Shell

- [ ] **Step 1: Update navigation tests**

Update `ShellNavigationPresenterTests` so `Transcribe Audio` appears after Dashboard and before History.

- [ ] **Step 2: Run red navigation test**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~ShellNavigationPresenterTests
```

Expected: fails until the presenter includes `Transcribe Audio`.

- [ ] **Step 3: Add navigation item and UI section**

Modify:

- `ShellNavigationPresenter.BuildItems()` to add `new("Transcribe Audio", "Transcribe Audio", "Audio")` after Dashboard.
- `MainWindow.xaml` to add `TranscribeAudioSectionPanel` with Choose Files, Start, Cancel, Clear, Remove, Retry, queue `ListView`, and detail `TextBox`.
- `MainWindow.xaml.cs` to add the section tag, show/hide logic, queue fields, picker handler, queue processor, selected-item detail refresh, and enable/disable rules.

Processing rules:

- Do not start while dictation recording/transcribing/inserting or another shell operation is active.
- Queue processing is sequential.
- Cancel resets the currently processing item to pending.
- Completed items select themselves and refresh History.
- Failed items show error text and can be retried.

- [ ] **Step 4: Verify focused navigation tests and build**

Run the focused navigation tests and Debug x64 build.

## Task 6: Docs, Review, Commit

- [ ] **Step 1: Update README/spec/plan**

Document the implemented Transcribe Audio MVP and remaining gaps:

- Multi-file picker and queue.
- Sequential local transcription into History.
- Supported formats depend on Windows Media Foundation codecs.
- Drag/drop, enhancement, per-file save/copy, and persistent queue restoration remain future gaps.

- [ ] **Step 2: Full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: all tests pass and Debug x64 build passes with 0 warnings/errors.

- [ ] **Step 3: Request review and fix findings**

Request subagent review against this plan and the macOS Transcribe Audio source. Fix all Critical and Important findings before committing.

- [ ] **Step 4: Commit**

Run:

```powershell
git add README.md `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\AudioFiles `
  VoiceInk.Windows\src\VoiceInk.Windows.Native\Audio\MediaFoundationAudioFileImportService.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\AudioFiles `
  VoiceInk.Windows\src\VoiceInk.Windows.Core\Shell\ShellNavigationPresenter.cs `
  VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Shell\ShellNavigationPresenterTests.cs `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml `
  VoiceInk.Windows\src\VoiceInk.Windows.App\MainWindow.xaml.cs `
  docs\superpowers\plans\2026-05-25-windows-transcribe-audio-mvp.md `
  docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add transcribe audio queue"
```
