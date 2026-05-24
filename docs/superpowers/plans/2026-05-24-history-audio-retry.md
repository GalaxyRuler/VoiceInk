# History Audio And Retry Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Store recording file paths in Windows history, expose selected-history audio playback/open actions, and retry selected audio through the current local transcription pipeline.

**Architecture:** Extend the Core history record with an optional audio file path, persist it through SQLite with migration, and save `AudioCaptureResult.FilePath` from dictation. Add a Core retry service that reuses settings, dictionary prompt/replacements, transcription, and post-processing without depending on WinUI. Keep playback/open-file behavior in the WinUI app because it is platform UI plumbing.

**Tech Stack:** .NET 10, WinUI 3 `MediaPlayerElement`, SQLite via `Microsoft.Data.Sqlite`, Whisper.net adapter, xUnit.

---

## Source Of Truth

macOS references:

- `VoiceInk/Models/Transcription.swift`: stores `audioFileURL`.
- `VoiceInk/Views/History/TranscriptionDetailView.swift`: shows `AudioPlayerView` only when the audio URL exists on disk.
- `VoiceInk/Views/AudioPlayerView.swift`: plays audio, shows in Finder, retranscribes current audio, and reports transient success/error status.
- `VoiceInk/Services/AudioFileTranscriptionService.swift`: retranscribes an existing audio URL, applies cleanup/dictionary/prompt/enhancement, copies audio to the app recordings folder, and saves a new history row.
- `VoiceInk/Services/LastTranscriptionService.swift`: retry-last validates the last audio file before retranscribing.

Windows slice behavior:

- Save the captured WAV path in each new completed history item.
- Existing databases migrate with `audio_file_path` set to null.
- History CSV includes the audio file path.
- Deleting a history row attempts to delete its stored audio file when it exists under the app recording directory.
- Selecting a history row with an existing audio file enables playback, open-folder, and retry.
- Retry selected audio uses current model/language/dictionary/settings, saves a new completed history row, and refreshes history. It does not paste text or run AI enhancement in this slice.

## File Map

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs`: add `AudioFilePath`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryCsvExporter.cs`: export audio path.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryRetryResult.cs`: retry status record.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryRetryService.cs`: selected-history retranscription pipeline.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`: save captured audio path.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`: add column, query, migration, and optional audio-file cleanup hook for delete.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add retry/open buttons and `MediaPlayerElement`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: wire selected-history audio controls, retry, and open in Explorer.
- Modify Core/Infrastructure tests in `VoiceInk.Windows/tests`.
- Modify `README.md` and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

---

## Task 1: Persist History Audio Path

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryCsvExporter.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryCsvExporterTests.cs`

- [ ] **Step 1: Write failing Infrastructure tests**

Add tests proving:

```csharp
SaveAndListRecentAsync_RoundTripsAudioFilePath()
MigrateLegacyDatabase_SetsAudioFilePathToNull()
```

Use a sample `TranscriptionHistoryItem(..., audioFilePath: @"C:\Recordings\sample.wav")` and assert `AudioFilePath` round-trips from `ListRecentAsync`, `SearchAsync`, and `GetLatestCompletedAsync`.

- [ ] **Step 2: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "AudioFilePath|MigrateLegacyDatabase"
```

Expected: build/test fails because `AudioFilePath` and `audio_file_path` are not implemented.

- [ ] **Step 3: Implement Core/SQLite audio path persistence**

Implementation details:

- Add nullable constructor parameter/property `string? AudioFilePath`.
- Add SQLite column `audio_file_path TEXT NULL`.
- Include `audio_file_path` in insert/select/read paths.
- Call `EnsureColumn(connection, "audio_file_path", "audio_file_path TEXT NULL")`.
- Use `ValueOrDbNull(item.AudioFilePath)`.

- [ ] **Step 4: Verify GREEN**

Run the same Infrastructure filtered command.

Expected: matching tests pass.

- [ ] **Step 5: Write failing Core tests for dictation save and CSV**

Add tests proving:

```csharp
StopAsync_SavesAudioFilePathInHistory()
Export_IncludesAudioFilePathColumn()
```

Expected saved history item receives the fake capture result path, and CSV header/value include `Audio File Path`.

- [ ] **Step 6: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "AudioFilePath|HistoryCsvExporterTests"
```

Expected: fails until dictation save and CSV exporter include the new property.

- [ ] **Step 7: Implement dictation save and CSV export**

Implementation details:

- In `DictationController.StopAsync`, pass `audioFilePath: audio.FilePath`.
- In `HistoryCsvExporter`, append `Audio File Path` to the header and item rows.

- [ ] **Step 8: Verify GREEN**

Run the same Core filtered command.

Expected: matching tests pass.

- [ ] **Step 9: Commit**

Commit message:

```text
feat(windows): persist history audio paths
```

---

## Task 2: Core Retry Selected Audio

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryRetryResult.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryRetryService.cs`
- Test: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryRetryServiceTests.cs`

- [ ] **Step 1: Write failing retry service tests**

Add tests for:

```csharp
RetryAsync_ReturnsFailureWhenAudioPathMissing()
RetryAsync_ReturnsFailureWhenAudioFileDoesNotExist()
RetryAsync_ReturnsFailureWhenModelPathMissing()
RetryAsync_TranscribesExistingAudioAndSavesNewHistoryItem()
RetryAsync_AppliesDictionaryAndCleanupSettings()
```

Use fake `ITranscriptionService`, `IHistoryStore`, `ISettingsStore`, and `IDictionaryStore`.

- [ ] **Step 2: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter HistoryRetryServiceTests
```

Expected: build fails because retry service/result do not exist.

- [ ] **Step 3: Implement retry result and service**

Implementation details:

- `HistoryRetryResult(bool Success, string Message, TranscriptionHistoryItem? Item = null)`.
- Validate nonblank `source.AudioFilePath`.
- Validate `File.Exists(source.AudioFilePath)`.
- Load settings and require model path.
- Load vocabulary and replacements.
- Call `transcriptionService.TranscribeAsync(new AudioCaptureResult(source.AudioFilePath, source.AudioDuration, 16000, 1), new TranscriptionOptions(settings.ModelPath, settings.Language, vocabularyPrompt), cancellationToken)`.
- Apply `TextPostProcessor.Process` with current cleanup settings and replacements.
- Save a new completed history item with new ID, UTC timestamp, source audio duration, transcription duration, original/final text, provider, language, model path, and `audioFilePath: source.AudioFilePath`.

- [ ] **Step 4: Verify GREEN**

Run the same `HistoryRetryServiceTests` command.

Expected: all retry service tests pass.

- [ ] **Step 5: Commit**

Commit message:

```text
feat(windows): retry selected history audio
```

---

## Task 3: WinUI History Audio Controls

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [ ] **Step 1: Add selected-history controls**

In the History section add:

- `Retry Selected`
- `Open Audio`
- `MediaPlayerElement x:Name="HistoryAudioPlayer" AreTransportControlsEnabled="True"`

Keep controls hidden/disabled unless the selected item has an existing audio file.

- [ ] **Step 2: Wire selection state**

Implementation details:

- Add `isRetryingHistory`.
- Add `SelectedHistoryItem()` and `SelectedHistoryAudioPath()` helpers.
- In `RefreshSelectedHistoryDetails`, display `Audio file: <path or Not recorded/Missing>` and set `HistoryAudioPlayer.Source = MediaSource.CreateFromUri(new Uri(path))` only when `File.Exists(path)`.
- Enable `RetryHistoryButton` and `OpenHistoryAudioButton` only when audio is available and no operation is active.

- [ ] **Step 3: Wire retry/open handlers**

Implementation details:

- `RetryHistoryButton_Click` calls `HistoryRetryService` with `new WhisperNetTranscriptionService()`, refreshes history, and shows the result message.
- `OpenHistoryAudioButton_Click` opens Explorer with `/select,"<path>"`.
- Existing history delete should also delete the selected audio file if it exists under `recordingsDirectory`; outside paths are left alone.

- [ ] **Step 4: Build verify**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [ ] **Step 5: Commit**

Commit message:

```text
feat(windows): expose history audio controls
```

---

## Task 4: Docs, Review, And Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: this plan

- [ ] **Step 1: Update docs**

Document history audio path storage, playback/open controls, selected retry, and remaining gaps: waveform/rate controls, AI re-enhance, global retry-last shortcut, batch actions, and Power Mode metadata.

- [ ] **Step 2: Full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [ ] **Step 3: Debug x64 build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [ ] **Step 4: Request review and fix Critical/Important findings**

Review the slice from `a56b404` through HEAD. Fix Critical and Important findings before moving on.

- [ ] **Step 5: Commit docs**

Commit message:

```text
docs(windows): note history audio retry
```

## Plan Self-Review

Spec coverage:

- Covers the macOS `audioFileURL` history capability with Windows-local file paths.
- Covers selected history playback/open/retry without adding cloud providers, AI enhancement, or commercial flows.
- Leaves waveform, playback speed, re-enhance, global retry-last shortcut, and batch history actions for later slices.

Placeholder scan:

- No placeholder tasks remain.

Type consistency:

- Uses `AudioFilePath`, `HistoryRetryService`, `HistoryRetryResult`, and existing `TranscriptionHistoryItem`, `AudioCaptureResult`, `TranscriptionOptions`, and `TextPostProcessor` names consistently.
