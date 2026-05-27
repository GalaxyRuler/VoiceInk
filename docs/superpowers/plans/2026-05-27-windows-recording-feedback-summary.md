# Windows Recording Feedback Summary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expose mute and media-pause recording feedback state in the Settings current-state summary.

**Architecture:** Update the existing Core `SettingsSectionPresenter` summary copy and focused presenter tests. No settings persistence, native feedback coordinator, or UI control wiring changes are required.

**Tech Stack:** .NET 10, xUnit, VoiceInk.Windows.Core.

---

### Task 1: Red Test Summary Copy

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Settings/SettingsSectionPresenterTests.cs`

- [x] **Step 1: Add failing test**

Add `Present_WithMuteAndMediaPause_DescribesFullRecordingFeedbackState`, asserting the Recording Feedback summary includes system mute and media pause resume delay.

- [x] **Step 2: Run focused RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~SettingsSectionPresenterTests.Present_WithMuteAndMediaPause_DescribesFullRecordingFeedbackState -nr:false -p:UseSharedCompilation=false
```

Expected RED: the summary only mentions start/stop sounds.

### Task 2: Presenter Summary

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/SettingsSectionPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Settings/SettingsSectionPresenterTests.cs`

- [x] **Step 1: Append mute/media details**

Build the Recording Feedback detail from scan-friendly fragments:

- Start/stop sound mode.
- `mutes system audio` when enabled.
- `pauses media and resumes after <delay>` when enabled.

- [x] **Step 2: Update existing expectations**

Default settings include system mute, so update the baseline summary expectation. Keep the built-in sound test focused by disabling system mute in that fixture.

- [x] **Step 3: Run focused GREEN**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~SettingsSectionPresenterTests -nr:false -p:UseSharedCompilation=false
```

Expected GREEN: all Settings presenter tests pass.

### Task 3: Docs, Verification, Commit

**Files:**
- Create: `docs/superpowers/specs/2026-05-27-windows-recording-feedback-summary.md`
- Create: `docs/superpowers/plans/2026-05-27-windows-recording-feedback-summary.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update parity docs**

Document that Settings current-state summary now includes mute/media pause state.

- [x] **Step 2: Run full verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
git diff --check
```

- [x] **Step 3: Commit**

Commit with:

```powershell
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/SettingsSectionPresenter.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Settings/SettingsSectionPresenterTests.cs docs/superpowers/specs/2026-05-27-windows-recording-feedback-summary.md docs/superpowers/plans/2026-05-27-windows-recording-feedback-summary.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md docs/superpowers/project-completion.md
git commit -m "feat(windows): summarize recording feedback state"
```
