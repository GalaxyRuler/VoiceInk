# Windows Power Mode URL Matching Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add sanitized browser URL pattern matching to Windows Power Mode rules.

**Architecture:** Extend Core Power Mode records and matcher logic, then feed the sanitized browser URL from the Native active-window target provider. Keep matching deterministic and backward compatible.

**Tech Stack:** .NET 10, WinUI 3, System.Text.Json settings persistence, xUnit.

---

### Task 1: Core Rule Matching

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeRule.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeTarget.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/PowerMode/PowerModeMatcher.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/PowerMode/PowerModeMatcherTests.cs`

- [ ] **Step 1: Write failing matcher tests**

Add tests for URL-only matching, URL sanitization, missing URL non-match, and combined process/title/URL matching.

- [ ] **Step 2: Run focused tests and verify red**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~PowerModeMatcherTests"
```

Expected: compile failure because URL fields do not exist.

- [ ] **Step 3: Implement Core fields and matcher logic**

Add `BrowserUrlPattern` to `PowerModeRule`, `BrowserUrl` to `PowerModeTarget`, and include URL matching in `PowerModeMatcher.IsSpecificMatch`.

- [ ] **Step 4: Run focused tests and verify green**

Run the same focused command. Expected: matcher tests pass.

### Task 2: Native Target and WinUI Editor

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/PowerMode/ActiveWindowPowerModeTargetProvider.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [ ] **Step 1: Feed browser URL into active targets**

Inject the browser URL reader into the active-window provider and include the sanitized URL in `PowerModeTarget`.

- [ ] **Step 2: Expose URL field in the Power Mode editor**

Add `PowerModeBrowserUrlTextBox`, fill it from selected rules, save it into rules, include it in list item summaries, and populate it from active targets.

- [ ] **Step 3: Run focused matcher tests**

Run the focused matcher tests again.

### Task 3: Full Verification and Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [ ] **Step 1: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

- [ ] **Step 2: Run Debug x64 build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

- [ ] **Step 3: Run whitespace check**

Run:

```powershell
git diff --check
```

- [ ] **Step 4: Update completion tracker and commit**

Commit only intentional source/docs changes.
