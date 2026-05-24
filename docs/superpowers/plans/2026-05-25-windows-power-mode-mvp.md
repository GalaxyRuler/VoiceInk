# Windows Power Mode MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the first source-runnable Windows Power Mode workflow: active app/window rules, session-scoped setting overrides, Power Mode history metadata, and a WinUI editor.

**Architecture:** Core owns Power Mode models, matching, settings overlay, and dictation-session application. Native owns Win32 active-window inspection behind a Core interface. WinUI owns rule editing and active-window quick fill.

**Tech Stack:** .NET 10, WinUI 3, xUnit, JSON settings, SQLite history, Win32 `GetForegroundWindow`, `GetWindowThreadProcessId`, and `GetWindowTextW`.

**Grounding:**

- macOS source of truth: `VoiceInk/PowerMode/PowerModeConfig.swift`, `VoiceInk/PowerMode/ActiveWindowService.swift`, `VoiceInk/PowerMode/PowerModeSessionManager.swift`, `VoiceInk/PowerMode/PowerModeView.swift`, `VoiceInk/PowerMode/PowerModeConfigView.swift`, and `VoiceInk/Transcription/Engine/VoiceInkEngine.swift`.
- Windows API grounding: Microsoft Learn documents `GetForegroundWindow` for foreground-window handle retrieval, `GetWindowThreadProcessId` for process id lookup, and `GetWindowTextW` for title bar text.
- Open-source adaptation: no paid provider locking, no license gates, no telemetry. Rules are local JSON settings; secrets remain in Credential Manager through existing enhancement infrastructure.

---

## Files

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeRule.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeTarget.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeMatchKind.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeResolution.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeMatcher.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IPowerModeTargetProvider.cs`.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/PowerMode/ActiveWindowPowerModeTargetProvider.cs`.
- Create `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/PowerMode/PowerModeMatcherTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/TranscriptionHistoryItem.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/History/HistoryCsvExporter.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/AudioFiles/AudioFileTranscriptionService.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Infrastructure/History/SqliteHistoryStore.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/AudioFiles/AudioFileTranscriptionServiceTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/History/HistoryCsvExporterTests.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/History/SqliteHistoryStoreTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Shell/ShellNavigationPresenter.cs`.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Shell/ShellNavigationPresenterTests.cs`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`.
- Modify `README.md`.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`.

## Task 1: Planning Docs

- [x] **Step 1: Update parity spec**

Record the Windows Power Mode MVP contract:

- Rules are local JSON settings and can be enabled, disabled, ordered, added, updated, and removed.
- Matching checks active process name and window title, case-insensitively.
- Rule order is priority; first enabled rule wins.
- Default rules are fallback only.
- Session settings are overlays, not persistent rewrites of base app settings.
- History stores Power Mode name and emoji.
- Browser URL matching, auto-send keys, Power Mode shortcuts, and recorder popover selection are later work.

- [x] **Step 2: Commit planning docs**

Run:

```powershell
git add docs\superpowers\plans\2026-05-25-windows-power-mode-mvp.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md
git commit -m "docs(windows): plan power mode"
```

Expected: docs-only commit.

## Task 2: Core Matcher Red/Green

- [x] **Step 1: Add failing matcher tests**

Create `PowerModeMatcherTests` for:

- enabled process-title contains rules match case-insensitively;
- disabled rules are ignored;
- first enabled matching rule wins;
- default rule applies only when no app/window rule matches;
- overrides produce a merged settings copy without mutating the base settings.

- [x] **Step 2: Run red tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~PowerMode
```

Expected: compile failure because Power Mode types do not exist.

- [x] **Step 3: Add Core matcher implementation**

Implement:

- `PowerModeRule` with id/name/emoji/enabled/default/match/override fields.
- `PowerModeTarget` with process name, window title, and process id.
- `PowerModeMatcher.Resolve(AppSettings, PowerModeTarget?)`.
- `AppSettings.PowerModeRules`.
- null/empty override handling so blank fields keep base settings.

- [x] **Step 4: Verify matcher tests pass**

Run the focused Power Mode Core test command. Expected: matcher tests pass.

## Task 3: Dictation Session Red/Green

- [x] **Step 1: Add failing dictation tests**

Cover:

- recording start captures the active target and applies matching model/language/enhancement overrides at stop;
- canceled recordings save the same active Power Mode metadata;
- provider failure or missing target falls back to base settings without blocking recording.

- [x] **Step 2: Run red dictation tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~DictationControllerTests
```

Expected: new tests fail until the controller stores session resolution.

- [x] **Step 3: Implement dictation session overlay**

Modify `DictationController` to:

- accept optional `IPowerModeTargetProvider`;
- resolve and store `PowerModeResolution` at `StartAsync`;
- validate the effective model path before capture starts;
- use effective settings for transcription, cleanup, enhancement, insertion, and canceled history;
- clear the session after stop/cancel/error.

- [x] **Step 4: Verify dictation tests pass**

Run the dictation-controller test command. Expected: all dictation tests pass.

## Task 4: History Metadata Red/Green

- [x] **Step 1: Add failing history tests**

Cover:

- `TranscriptionHistoryItem` round-trips Power Mode name/emoji through SQLite.
- legacy databases migrate with null Power Mode fields.
- CSV exports a `Power Mode` column rendered as `emoji name`.
- audio-file transcription saves null Power Mode metadata.

- [x] **Step 2: Run red history tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~HistoryCsvExporterTests|FullyQualifiedName~SqliteHistoryStoreTests|FullyQualifiedName~AudioFileTranscriptionServiceTests"
```

Expected: tests fail until schema/export/item changes exist.

- [x] **Step 3: Implement history metadata**

Add `PowerModeName` and `PowerModeEmoji` to history item, SQLite insert/select/search/schema migration, CSV header/rows, and UI metadata display.

- [x] **Step 4: Verify history tests pass**

Run the filtered history command. Expected: tests pass.

## Task 5: Native Active Window And Shell UI

- [x] **Step 1: Add the native target provider**

Implement `ActiveWindowPowerModeTargetProvider` with safe P/Invoke:

- return null when no foreground window exists;
- read process id through `GetWindowThreadProcessId`;
- read title through `GetWindowTextW`;
- read process name through `Process.GetProcessById`;
- catch process-access failures and still return title/process id when possible.

- [x] **Step 2: Wire controller construction**

Pass the native provider into `CreateController` so dictation sessions can resolve Power Mode without UI coupling.

- [x] **Step 3: Add WinUI Power Mode section**

Add navigation and controls:

- rule list;
- name/emoji/process/title fields;
- enabled/default toggles;
- override fields for model path, language, enhancement enabled, prompt, trailing space, filler words, punctuation cleanup, lowercase;
- add/update/remove/move up/move down buttons;
- refresh active window and use active window buttons.

- [x] **Step 4: Persist UI edits through settings**

Load rules into the section, save rules through `SaveSettingsAsync`, and keep rule list state refreshed after add/update/remove/reorder.

- [x] **Step 5: Verify build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with zero errors.

## Task 6: Docs, Review, And Commit

- [x] **Step 1: Update README and parity spec**

Document source-runnable Power Mode behavior and remaining gaps.

- [ ] **Step 2: Run focused tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln --filter "FullyQualifiedName~PowerMode|FullyQualifiedName~DictationControllerTests|FullyQualifiedName~HistoryCsvExporterTests|FullyQualifiedName~SqliteHistoryStoreTests"
```

Expected: focused tests pass.

- [ ] **Step 3: Request review and fix Critical/Important findings**

Ask a subagent to review the Power Mode slice for parity, lifecycle correctness, persistence migration, and secret safety.

- [ ] **Step 4: Run final verification**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: full tests pass and Debug x64 build succeeds.

- [ ] **Step 5: Commit implementation**

Run:

```powershell
git add VoiceInk.Windows README.md docs\superpowers\specs\2026-05-24-windows-open-source-parity-design.md docs\superpowers\plans\2026-05-25-windows-power-mode-mvp.md
git diff --check --cached
git commit -m "feat(windows): add power mode rules"
```

Expected: implementation commit with no whitespace errors.
