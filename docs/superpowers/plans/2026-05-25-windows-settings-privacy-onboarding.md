# Windows Settings Privacy And Onboarding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add macOS-aligned Settings controls for reset onboarding plus transcript/audio privacy cleanup.

**Architecture:** Keep cleanup behavior in Core, persistence queries in Infrastructure, and WinUI as a thin shell that saves settings, confirms destructive actions, and refreshes history. History cleanup works through `IHistoryStore` so the UI does not issue SQL directly.

**Tech Stack:** .NET 10, WinUI 3, System.Text.Json settings, Microsoft.Data.Sqlite history, xUnit.

---

## File Structure

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`: add cleanup setting properties and include them in equality/hash behavior.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IHistoryStore.cs`: add old-history listing and audio reference clearing operations.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Privacy/PrivacyCleanupPreview.cs`: immutable preview for audio cleanup count/size.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Privacy/PrivacyCleanupResult.cs`: immutable cleanup result for status messages.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Privacy/PrivacyCleanupService.cs`: retention cutoff calculation and transcript/audio cleanup orchestration.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`: implement old-history listing and audio reference clearing.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`: prove cleanup settings round-trip.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`: prove old-history listing and audio reference clearing.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Privacy/PrivacyCleanupServiceTests.cs`: prove transcript cleanup deletes rows/audio and audio cleanup preserves transcript text.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add Privacy controls and Reset Onboarding button.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: wire controls, settings persistence, dialogs, cleanup service calls, status text, and enablement.
- Modify `README.md`, `docs/superpowers/project-completion.md`, and `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`: document completed behavior and update the project bar.

## Task 1: Core And Infrastructure Privacy Cleanup

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IHistoryStore.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Privacy/PrivacyCleanupPreview.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Privacy/PrivacyCleanupResult.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Privacy/PrivacyCleanupService.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Privacy/PrivacyCleanupServiceTests.cs`

- [ ] **Step 1: Write failing settings round-trip test**

Add cleanup fields to `JsonSettingsStoreTests.SaveAsync_PersistsSettings`:

```csharp
IsTranscriptionCleanupEnabled = true,
TranscriptionRetentionMinutes = 60,
IsAudioCleanupEnabled = true,
AudioRetentionPeriod = 14,
```

- [ ] **Step 2: Run settings test and verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "JsonSettingsStoreTests.SaveAsync_PersistsSettings"
```

Expected: compile failure because the cleanup settings do not exist on `AppSettings`.

- [ ] **Step 3: Write failing history store tests**

Add tests:

```csharp
[Fact]
public async Task ListOlderThanAsync_ReturnsOnlyItemsOlderThanCutoff()
{
    using var temp = new TempDirectory();
    var store = new SqliteHistoryStore(Path.Combine(temp.Path, "history.db"));
    var old = new TranscriptionHistoryItem(Guid.NewGuid(), DateTimeOffset.Parse("2026-05-25T10:00:00Z"), "old", "local-whisper", TimeSpan.Zero, TimeSpan.Zero);
    var newItem = old with { Id = Guid.NewGuid(), CreatedAt = DateTimeOffset.Parse("2026-05-25T12:00:00Z"), Text = "new", OriginalText = "new" };
    await store.SaveAsync(old, CancellationToken.None);
    await store.SaveAsync(newItem, CancellationToken.None);

    var results = await store.ListOlderThanAsync(DateTimeOffset.Parse("2026-05-25T11:00:00Z"), CancellationToken.None);

    Assert.Equal(old, Assert.Single(results));
}

[Fact]
public async Task ClearAudioFilePathAsync_ClearsOnlySelectedRows()
{
    using var temp = new TempDirectory();
    var store = new SqliteHistoryStore(Path.Combine(temp.Path, "history.db"));
    var cleared = new TranscriptionHistoryItem(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-2), "clear", "local-whisper", TimeSpan.Zero, TimeSpan.Zero, audioFilePath: @"C:\Audio\clear.wav");
    var kept = cleared with { Id = Guid.NewGuid(), Text = "keep", OriginalText = "keep", AudioFilePath = @"C:\Audio\keep.wav" };
    await store.SaveAsync(cleared, CancellationToken.None);
    await store.SaveAsync(kept, CancellationToken.None);

    var count = await store.ClearAudioFilePathAsync([cleared.Id], CancellationToken.None);

    Assert.Equal(1, count);
    var rows = await store.ListRecentAsync(10, CancellationToken.None);
    Assert.Null(rows.Single(row => row.Id == cleared.Id).AudioFilePath);
    Assert.Equal(@"C:\Audio\keep.wav", rows.Single(row => row.Id == kept.Id).AudioFilePath);
}
```

- [ ] **Step 4: Run history tests and verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "ListOlderThanAsync|ClearAudioFilePathAsync"
```

Expected: compile failure because the new `IHistoryStore` methods do not exist.

- [ ] **Step 5: Write failing cleanup service tests**

Create `PrivacyCleanupServiceTests` covering:

```csharp
[Fact]
public async Task RunTranscriptCleanupAsync_DeletesOldRowsAndAssociatedAudioFiles()
[Fact]
public async Task RunAudioCleanupAsync_DeletesOldAudioAndPreservesTranscript()
[Fact]
public async Task PreviewAudioCleanupAsync_ReturnsEligibleFileCountAndBytes()
```

Use temp audio files and a fake in-memory `IHistoryStore` so the service can be tested without SQLite.

- [ ] **Step 6: Run cleanup service tests and verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "PrivacyCleanupServiceTests"
```

Expected: compile failure because `PrivacyCleanupService` does not exist.

- [ ] **Step 7: Implement minimal Core settings and cleanup service**

Add `AppSettings` properties with defaults:

```csharp
public bool IsTranscriptionCleanupEnabled { get; init; }
public int TranscriptionRetentionMinutes { get; init; } = 24 * 60;
public bool IsAudioCleanupEnabled { get; init; }
public int AudioRetentionPeriod { get; init; } = 7;
```

Add `PrivacyCleanupService` using `TimeProvider`, `ISettingsStore`, and `IHistoryStore`. Clamp negative retentions to zero. Transcript cleanup deletes old audio files first, then deletes old history rows. Audio cleanup deletes old audio files and clears only the successfully deleted rows' audio references.

- [ ] **Step 8: Implement minimal SQLite methods**

Implement `ListOlderThanAsync(cutoff)` with `created_at_utc_ticks < cutoff.UtcDateTime.Ticks`, ordered newest first, and `ClearAudioFilePathAsync(ids)` with parameterized `UPDATE transcriptions SET audio_file_path = NULL WHERE id IN (...)`.

- [ ] **Step 9: Run focused tests and verify GREEN**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "PrivacyCleanupServiceTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "JsonSettingsStoreTests.SaveAsync_PersistsSettings|ListOlderThanAsync|ClearAudioFilePathAsync"
```

Expected: all selected tests pass.

## Task 2: WinUI Settings Controls

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [ ] **Step 1: Add Settings controls**

Add a `Privacy` group under Settings with transcript/audio cleanup controls and a `General` group with `Reset Onboarding`.

- [ ] **Step 2: Wire settings load/save**

Set controls in `ApplySettingsToUiAsync`, read them in `CurrentSettingsAsync`, and add an `Apply Cleanup Settings` button that calls `SaveSettingsAsync`.

- [ ] **Step 3: Wire reset onboarding**

Add a `ContentDialog` titled `Reset Onboarding?`. On primary result, save settings with `HasCompletedOnboarding = false` and show status `Onboarding will show on next launch`.

- [ ] **Step 4: Wire manual cleanup actions**

Transcript cleanup: save current settings, call `RunTranscriptCleanupAsync`, refresh history/metrics best effort, and show deleted counts.

Audio cleanup: save current settings, call `PreviewAudioCleanupAsync`, show a confirmation dialog with count/size, call `RunAudioCleanupAsync` only after primary result, then refresh history and selected audio state.

- [ ] **Step 4a: Wire automatic cleanup cadence**

Run configured cleanup on launch, after completed recordings, and from a daily in-app timer while transcript cleanup or audio-only cleanup is enabled. Transcript cleanup takes precedence over audio-only cleanup. Cleanup must only delete direct app-created recording WAV files in the recordings directory and keep transcript rows when an app-owned audio file cannot be deleted.

- [ ] **Step 5: Run app build and fix compile issues**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds.

## Task 3: Docs, Tracker, Review, Commit

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [ ] **Step 1: Document the new Settings privacy controls**

Add a short README note that cleanup is local-only, transcript cleanup deletes history and audio, audio cleanup preserves transcript text, and API keys are unaffected.

- [ ] **Step 2: Update the completion bar**

Increase Settings and Onboarding percentages modestly, update the current slice bar to `Settings privacy/onboarding`, and keep the remaining gaps ordered by priority.

- [ ] **Step 3: Run full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Expected: tests pass, build succeeds, and only existing line-ending warnings appear if any.

- [ ] **Step 4: Request code review**

Dispatch a reviewer with the slice requirements and diff. Fix all Critical and Important findings before continuing.

- [ ] **Step 5: Commit**

Run:

```powershell
git add README.md docs/superpowers/project-completion.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/plans/2026-05-25-windows-settings-privacy-onboarding.md VoiceInk.Windows/src/VoiceInk.Windows.Core VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History VoiceInk.Windows/src/VoiceInk.Windows.App VoiceInk.Windows/tests
git commit -m "feat(windows): add privacy cleanup settings"
```
