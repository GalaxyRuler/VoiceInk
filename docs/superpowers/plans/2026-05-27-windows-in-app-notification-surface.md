# Windows In-App Notification Surface Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a tested, Windows-native transient notification surface for actionable VoiceInk status messages.

**Architecture:** Put message classification in Core so it can be tested without UI automation. Render notifications in WinUI with an `InfoBar` connected to the existing `RefreshUiFromControllerState` status flow.

**Tech Stack:** .NET 10, xUnit, WinUI 3 `InfoBar`.

---

### Task 1: Core Presenter

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Notifications/AppNotificationKind.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Notifications/AppNotificationPresentation.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Notifications/AppNotificationPresenter.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Notifications/AppNotificationPresenterTests.cs`

- [x] **Step 1: Write failing presenter tests**

Add tests for passive-status suppression and info/success/warning/error classification.

- [x] **Step 2: Run focused tests red**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~AppNotificationPresenterTests"
```

Expected: compile failure until Core notification types exist.

- [x] **Step 3: Implement presenter**

Add the notification kind enum, presentation record, and status classifier.

- [x] **Step 4: Run focused tests green**

Expected: all notification presenter tests pass.

### Task 2: WinUI Surface

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add InfoBar surface**

Add an initially closed `InfoBar` above the footer status text.

- [x] **Step 2: Wire status flow**

Show notifications for actionable status overrides, controller errors, warnings, and idle hotkey warnings. Leave passive state text out of the notification surface.

- [x] **Step 3: Add auto-dismiss timer**

Use presenter durations and map Core notification kinds to WinUI `InfoBarSeverity`.

- [x] **Step 4: Build app project**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\src\VoiceInk.Windows.App\VoiceInk.Windows.App.csproj -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 warnings and 0 errors.

### Task 3: Docs And Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update tracker**

Update shell/tray or settings parity notes and set the current slice to in-app notification surface.

- [x] **Step 2: Run full verification**

Run full solution tests, Debug x64 build, and `git diff --check`.

- [x] **Step 3: Commit**

Commit with:

```powershell
git add docs/superpowers VoiceInk.Windows
git commit -m "feat(windows): add in-app notification surface"
```
