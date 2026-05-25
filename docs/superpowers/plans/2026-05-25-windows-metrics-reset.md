# Windows Metrics Reset Implementation Plan

Date: 2026-05-25

## Goal

Ship a confirmed local Metrics reset control in the Windows Metrics section.

## Task 1: Store Contract And Red Test

Files:

- `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/ISessionMetricStore.cs`
- `VoiceInk.Windows/src/VoiceInk.Windows.Core/Metrics/DisabledSessionMetricStore.cs`
- `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/Metrics/SqliteSessionMetricStore.cs`
- `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Metrics/SqliteSessionMetricStoreTests.cs`

Steps:

1. Add a focused SQLite test proving `ClearAsync` removes saved metrics, model performance rows, and duplicate-transcription markers while allowing future saves.
2. Run the focused test and confirm it fails because `ClearAsync` does not exist.
3. Add `ClearAsync(CancellationToken)` to `ISessionMetricStore`.
4. Implement a no-op in `DisabledSessionMetricStore`.
5. Implement `DELETE FROM session_metrics;` in `SqliteSessionMetricStore`.
6. Update test fakes implementing `ISessionMetricStore`.
7. Run the focused metrics store tests.

Verification:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~SqliteSessionMetricStoreTests"
```

## Task 2: WinUI Reset Control

Files:

- `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

Steps:

1. Add a `Reset Metrics` button next to Refresh and Export.
2. Wire the button to a confirmation `ContentDialog`.
3. On confirm, call `sessionMetricStore.ClearAsync`, refresh Metrics, and show `Metrics reset`.
4. On cancel, show `Metrics reset canceled`.
5. Disable Reset while settings are not loaded or any operation is active.

Verification:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

## Task 3: Docs, Review, And Commit

Files:

- `docs/superpowers/project-completion.md`
- `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- `docs/superpowers/specs/2026-05-25-windows-metrics-reset-design.md`
- `docs/superpowers/plans/2026-05-25-windows-metrics-reset.md`

Steps:

1. Mark Metrics reset as implemented and keep remaining Metrics gaps focused on visual dashboard parity and diagnostics expansion.
2. Update the completion bar current slice.
3. Review the diff locally for regressions.
4. Run full tests and Debug x64 build.
5. Commit with `feat(windows): add metrics reset control`.

Verification:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```
