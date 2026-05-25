# Windows Failed Dictation History Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Save failed recorder-stop dictation attempts to History when audio capture has already stopped.

**Architecture:** Keep persistence behind `IHistoryStore`. Add a best-effort failed-history helper in `DictationController` analogous to the existing canceled-history helper.

**Tech Stack:** .NET 10, Core dictation services, xUnit.

---

### Task 1: Failed History Red/Green

Files:

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`

Steps:

1. Extend `StopAsync_TranscriptionFailureSetsError` to assert a failed History row.
2. Run that focused test and confirm it fails because no failed row is saved.
3. Add a failed-history helper and call it when post-capture stop work fails before completed history is saved.
4. Preserve existing capture-stop failure behavior: no audio result means no failed row.
5. Run focused dictation failure tests.

### Task 2: Docs And Verification

Files:

- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Add: `docs/superpowers/specs/2026-05-25-windows-failed-dictation-history-design.md`
- Add: `docs/superpowers/plans/2026-05-25-windows-failed-dictation-history.md`

Verification:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictationControllerTests.StopAsync_TranscriptionFailureSetsError|FullyQualifiedName~DictationControllerTests.StopAsync_CaptureStopFailureSetsError"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Commit:

```powershell
git commit -m "fix(windows): save failed dictation history"
```
