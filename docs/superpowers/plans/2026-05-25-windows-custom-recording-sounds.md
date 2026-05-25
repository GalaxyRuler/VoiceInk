# Windows Custom Recording Sounds Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-style custom start/stop recording sound import, reset, test, persistence, and playback to the Windows Settings experience.

**Architecture:** Keep selection, import policy, validation outcomes, and recording-session snapshots in Core. Keep NAudio decoding/playback and WinUI picker interaction in Native/App. Persist custom sound choices in `AppSettings` and General Settings backups.

**Tech Stack:** .NET 10, WinUI 3, NAudio, xUnit, existing JSON settings and backup services.

---

## File Map

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recording/RecordingSoundKind.cs` for start/stop identifiers.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recording/RecordingSoundModeSettings.cs` for persisted mode constants and normalization.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recording/RecordingSoundPlaybackSettings.cs` for sound playback snapshots.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recording/RecordingSoundSettings.cs` for projecting settings from `AppSettings`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recording/CustomRecordingSoundImporter.cs` for validation/import/reset policy.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recording/IRecordingSoundFileProbe.cs` and `IRecordingSoundFileSystem.cs` for testable duration/file operations.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recording/IRecordingSoundFeedback.cs` so playback receives `RecordingSoundPlaybackSettings`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recording/RecordingFeedbackCoordinator.cs` to snapshot and pass start/stop sound settings.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs` to add new persisted sound fields.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recording/CustomRecordingSoundImporterTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recording/RecordingFeedbackCoordinatorTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Recording/LocalRecordingSoundFileSystem.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Recording/WindowsRecordingSoundFileProbe.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Native/Recording/WindowsRecordingSoundFeedback.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml` and `MainWindow.xaml.cs` for Settings controls, pickers, test/reset handlers, and service wiring.
- Update `README.md`, the parity spec, and `docs/superpowers/project-completion.md`.

## Task 1: Core Sound Models And Importer

- [ ] Write failing tests in `CustomRecordingSoundImporterTests.cs` for mode normalization, invalid extension, missing file, too-long duration, stable destination naming, replacing the previous app-owned file, and reset.
- [ ] Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~CustomRecordingSoundImporterTests"
```

Expected: fail because the types do not exist.

- [ ] Add the Core sound model/importer files.
- [ ] Re-run the focused tests.

Expected: pass.

## Task 2: Settings, Backup, And Feedback Snapshot

- [ ] Add failing assertions to `RecordingFeedbackCoordinatorTests.cs` that start and stop playback receive the settings captured at `BeginAsync`.
- [ ] Add failing persistence assertions to `JsonSettingsStoreTests.cs` for `StartSoundMode`, `StopSoundMode`, `CustomStartSoundPath`, and `CustomStopSoundPath`.
- [ ] Add failing backup export/merge assertions to `VoiceInkSettingsBackupTests.cs`.
- [ ] Run focused tests for these files.

Expected: fail because the settings fields and interface changes are not implemented.

- [ ] Add the new `AppSettings` fields, equality/hash participation, backup behavior through existing whole-record export/merge, and feedback coordinator snapshot plumbing.
- [ ] Update fake sound feedback implementations.
- [ ] Re-run the focused tests.

Expected: pass.

## Task 3: Native Sound Validation And Playback

- [ ] Add `LocalRecordingSoundFileSystem` and `WindowsRecordingSoundFileProbe`.
- [ ] Update `WindowsRecordingSoundFeedback` to play custom files with NAudio when `Mode == custom` and a valid path exists, dispose playback objects on `PlaybackStopped`, and fall back to Windows system sounds on failure.
- [ ] Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\src\VoiceInk.Windows.Native\VoiceInk.Windows.Native.csproj -c Debug -p:Platform=x64
```

Expected: build succeeds.

## Task 4: WinUI Settings Controls

- [ ] Add start/stop sound ComboBoxes, status TextBlocks, and `Test`, `Choose`, `Reset` buttons to the existing Recording Feedback group.
- [ ] Construct a `CustomRecordingSoundImporter` in `MainWindow`.
- [ ] Wire `Choose` buttons to a `FileOpenPicker` with `.wav`, `.mp3`, `.aiff`, `.aif`, initialized with the main window handle.
- [ ] Save settings immediately after successful choose/reset and refresh the sound controls.
- [ ] Wire `Test` buttons to `WindowsRecordingSoundFeedback` using the current UI selection.
- [ ] Include new settings in `CurrentSettingsAsync`.
- [ ] Run the app project build:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

Expected: build succeeds.

## Task 5: Docs, Verification, Review, Commit

- [ ] Update README, parity spec, and completion tracker with the new custom recording sound behavior.
- [ ] Run focused Core and Infrastructure tests touched by the slice.
- [ ] Run full solution tests.
- [ ] Run full Debug x64 solution build.
- [ ] Request code review for the slice.
- [ ] Fix all Critical and Important review findings.
- [ ] Run verification again after fixes.
- [ ] Commit with message:

```powershell
git add VoiceInk.Windows docs README.md
git commit -m "feat(windows): add custom recording sounds"
```
