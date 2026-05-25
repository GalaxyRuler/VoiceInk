# Windows Recording Feedback Implementation Plan

**Date:** 2026-05-25

**Goal:** Port the macOS Recording Feedback settings into the Windows fork as a source-runnable, open-source Windows implementation.

**Architecture:** Keep settings and lifecycle orchestration in Core so behavior is testable without WinUI. Keep Windows sound playback, default-output mute/restore, and Global System Media Transport Controls pause/resume in Native adapters. Keep the WinUI shell responsible only for settings controls and connecting the coordinator to recording start/stop/cancel.

## File Structure

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`: recording feedback settings.
- Add `VoiceInk.Windows/src/VoiceInk.Windows.Core/Recording/RecordingFeedback*.cs`: coordinator and interfaces.
- Add `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Recording/RecordingFeedbackCoordinatorTests.cs`: lifecycle behavior tests.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`: persistence coverage.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Backup/VoiceInkSettingsBackupTests.cs`: backup/import coverage.
- Add Native recording-feedback adapters under `VoiceInk.Windows/src/VoiceInk.Windows.Native/Recording/`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: Settings controls.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: load/save controls and lifecycle wiring.
- Update README, parity spec, and project completion tracker.

## Task 1: Core Settings And Coordinator

- [x] **Step 1: Write failing Core tests**

Test that `RecordingFeedbackCoordinator`:

- Plays start sound, mutes system audio, and pauses media on begin when enabled.
- Plays stop sound and restores audio/media after the session delay on complete.
- Restores audio/media without stop sound on cancel.
- Does nothing for disabled settings.
- Uses the settings snapshot captured at begin rather than later UI changes.

- [x] **Step 2: Verify RED**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "RecordingFeedbackCoordinatorTests"
```

Expected: compile failures because the coordinator and interfaces do not exist yet.

- [x] **Step 3: Implement Core support**

Add `AppSettings` fields:

- `IsSoundFeedbackEnabled = true`
- `IsSystemMuteEnabled = true`
- `IsPauseMediaEnabled = false`
- `AudioResumptionDelaySeconds = 0.0`

Add a coordinator that snapshots settings on begin and calls small interfaces for sound, system mute/restore, and media pause/resume.

- [x] **Step 4: Verify GREEN**

Run the same focused Core test command. Expected: selected tests pass.

## Task 2: Persistence, Backup, And WinUI Controls

- [x] **Step 1: Add failing persistence and backup assertions**

Cover JSON persistence and General Settings backup/import for the new fields.

- [x] **Step 2: Add Settings controls**

Add a `Recording Feedback` group with:

- `Sound Feedback`
- `Mute Audio While Recording`
- `Pause Media While Recording`
- `Resume Delay` choices: `0s`, `1s`, `2s`, `3s`, `4s`, `5s`
- `Apply Recording Feedback`

- [x] **Step 3: Wire settings load/save**

Populate controls from `AppSettings`, include them in `CurrentSettingsAsync`, and disable them during active recording/operations.

- [x] **Step 4: Verify**

Run focused settings/backup tests, then build.

## Task 3: Native Feedback Adapters And Lifecycle

- [x] **Step 1: Add Windows adapters**

Implement:

- Windows system sounds for start/stop.
- Default render endpoint mute/restore through existing NAudio/CoreAudio APIs.
- Opt-in Global System Media Transport Controls pause/resume.

- [x] **Step 2: Wire recording lifecycle**

Begin feedback after validation/settings save and before capture start. Restore when capture stops, and also on cancel, start failure, and window close. Play stop sound only after successful text insertion.

- [x] **Step 3: Verify**

Run focused Core tests, full solution tests, and x64 Debug build.

## Task 4: Docs, Review, Commit

- [x] **Step 1: Update docs and completion bar**

Document behavior, Windows-specific media-pause caveat, and local-only privacy boundary.

- [x] **Step 2: Request review**

Ask for review focused on lifecycle restore safety, settings persistence, default behavior parity, and Windows adapter risk. Fix all Critical/Important findings.

- [x] **Step 3: Final verification**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

- [x] **Step 4: Commit**

```powershell
git commit -m "feat(windows): add recording feedback settings"
```
