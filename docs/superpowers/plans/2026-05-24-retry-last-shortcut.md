# Retry Last Shortcut Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a macOS-style Retry Last Transcription action to the Windows fork, available from the shell and an optional configurable global shortcut.

**Architecture:** Reuse the existing history-audio retry service rather than adding a parallel pipeline. Core owns "find latest completed item and retry it"; the WinUI shell owns command routing and clipboard feedback. Shortcut settings remain data-only in Core and native registration remains behind the existing Win32 hotkey service.

**Tech Stack:** .NET 10, WinUI 3, xUnit, SQLite history store, existing repo-local SDK.

---

## Files

- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs` to persist `RetryLastTranscriptionHotkey`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shortcuts/GlobalShortcutAction.cs` and `GlobalShortcutSettings.cs` to register the optional retry-last action.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryRetryService.cs` to add `RetryLatestAsync`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs` for retry-last shortcut registration and duplicate validation.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryRetryServiceTests.cs` for latest-history retry behavior.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs` to prove settings JSON round-trips the retry shortcut.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml` to add a Retry Last button and shortcut field.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs` to route button/hotkey commands, save current settings, retry the newest completed item, copy the retried text, and refresh/select history.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md` and `VoiceInk.Windows/README.md` after implementation.

## Task 1: Planning Docs

- [ ] **Step 1: Update the parity spec**

Record that retry-last is the active shortcut/history slice and that it will be implemented as a local-only, open-source history retry command.

- [ ] **Step 2: Commit the plan**

Run:

```powershell
git add docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md docs\superpowers\plans\2026-05-24-retry-last-shortcut.md
git commit -m "docs(windows): plan retry last shortcut"
```

Expected: commit succeeds with only docs changes.

## Task 2: Core Red Tests

- [ ] **Step 1: Add shortcut registration tests**

In `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shortcuts/GlobalShortcutTests.cs`, update `BuildRegistrations_UsesPrimaryAndOptionalHotkeys` to set `RetryLastTranscriptionHotkey = "Ctrl+Alt+R"` and assert a fourth registration with `GlobalShortcutAction.RetryLastTranscription`. Add a duplicate-assignment test with `RetryLastTranscriptionHotkey = "Ctrl+Alt+Space"` expecting `Retry Last Transcription already uses Ctrl+Alt+Space.`.

- [ ] **Step 2: Add retry latest tests**

In `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryRetryServiceTests.cs`, add:

```csharp
[Fact]
public async Task RetryLatestAsync_ReturnsFailureWhenNoCompletedTranscriptionExists()
{
    var service = new HistoryRetryService(
        new FakeTranscriptionService(),
        new FakeHistoryStore(),
        new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }));

    var result = await service.RetryLatestAsync(CancellationToken.None);

    Assert.False(result.Success);
    Assert.Equal("No transcription available", result.Message);
}

[Fact]
public async Task RetryLatestAsync_RetriesLatestCompletedHistoryItem()
{
    using var audio = new TempAudioFile();
    var history = new FakeHistoryStore();
    var source = HistoryItem(audio.Path) with
    {
        Status = TranscriptionHistoryStatus.Completed,
        AudioDuration = TimeSpan.FromSeconds(7)
    };
    history.Items.Add(source);
    var transcription = new FakeTranscriptionService(new TranscriptionResult(
        " retried text ",
        TimeSpan.FromMilliseconds(50),
        "local-whisper"));
    var service = new HistoryRetryService(
        transcription,
        history,
        new FakeSettingsStore(new AppSettings { ModelPath = "ggml-base.en.bin" }));

    var result = await service.RetryLatestAsync(CancellationToken.None);

    Assert.True(result.Success);
    Assert.Equal("Retry transcription saved", result.Message);
    Assert.NotNull(result.Item);
    Assert.Equal("retried text", result.Item.Text);
    Assert.Equal(audio.Path, result.Item.AudioFilePath);
    Assert.Equal(TimeSpan.FromSeconds(7), transcription.LastAudio?.Duration);
}
```

- [ ] **Step 3: Add settings round-trip test expectation**

In `JsonSettingsStoreTests.SaveAsync_PersistsSettings`, add `RetryLastTranscriptionHotkey = "Ctrl+Alt+R"`.

- [ ] **Step 4: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~GlobalShortcutTests|FullyQualifiedName~HistoryRetryServiceTests"
```

Expected: compile failure or test failure because `RetryLastTranscriptionHotkey`, `RetryLastTranscription`, and `RetryLatestAsync` are missing.

## Task 3: Core Implementation

- [ ] **Step 1: Add settings and shortcut action**

Add to `AppSettings`:

```csharp
public string RetryLastTranscriptionHotkey { get; init; } = string.Empty;
```

Add to `GlobalShortcutAction`:

```csharp
RetryLastTranscription
```

- [ ] **Step 2: Register the optional shortcut**

In `GlobalShortcutSettings.BuildRegistrations`, add:

```csharp
AddRegistration(
    settings.RetryLastTranscriptionHotkey,
    GlobalShortcutAction.RetryLastTranscription,
    "Retry Last Transcription",
    required: false,
    registrations,
    errors,
    usedShortcuts);
```

- [ ] **Step 3: Add latest retry**

In `HistoryRetryService`, add:

```csharp
public async Task<HistoryRetryResult> RetryLatestAsync(CancellationToken cancellationToken)
{
    var source = await historyStore.GetLatestCompletedAsync(cancellationToken);
    return source is null
        ? new HistoryRetryResult(false, "No transcription available")
        : await RetryAsync(source, cancellationToken);
}
```

- [ ] **Step 4: Run focused Core/Infrastructure tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~GlobalShortcutTests|FullyQualifiedName~HistoryRetryServiceTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter FullyQualifiedName~JsonSettingsStoreTests
```

Expected: all selected tests pass.

## Task 4: WinUI Wiring

- [ ] **Step 1: Add shell controls**

In `MainWindow.xaml`, add a `RetryLastHotkeyTextBox` under Shortcuts with header `Retry Last Transcription`, placeholder `Ctrl+Alt+R`, and add a `RetryLastButton` to the History command row.

- [ ] **Step 2: Load and save settings**

In `MainWindow.xaml.cs`, set `RetryLastHotkeyTextBox.Text = settings.RetryLastTranscriptionHotkey` during initialization and include it in `CurrentSettingsAsync` when `includeShortcutFields` is true.

- [ ] **Step 3: Route button and hotkey**

Add a `RetryLastButton_Click` handler and route `GlobalShortcutAction.RetryLastTranscription` in `HotkeyService_HotkeyPressed` to a new `RetryLastHistoryAsync` method.

- [ ] **Step 4: Implement command behavior**

`RetryLastHistoryAsync` should:

- Ignore duplicate invocations while `isRetryingHistory` is true.
- Save current non-shortcut settings before retrying.
- Call `historyRetryService.RetryLatestAsync`.
- Refresh history and select the saved retry item.
- Copy `result.Item.Text` to the clipboard on success with `DataPackage.SetText`, `Clipboard.SetContent`, and `Clipboard.Flush`.
- Report `Retry transcription copied` on success and the retry service message on failure.

- [ ] **Step 5: Run app build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build passes with 0 warnings and 0 errors.

## Task 5: Docs, Review, Commit

- [ ] **Step 1: Update docs**

Update the parity spec and README to list Retry Last as implemented, including the optional shortcut field and clipboard result behavior.

- [ ] **Step 2: Run full verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: test and build pass.

- [ ] **Step 3: Request code review**

Dispatch a reviewer against the implementation diff. Fix all Critical and Important findings before committing.

- [ ] **Step 4: Commit implementation**

Run:

```powershell
git add VoiceInk.Windows docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "feat(windows): add retry last transcription shortcut"
```

Expected: commit succeeds; only `.superpowers/` remains untracked.
