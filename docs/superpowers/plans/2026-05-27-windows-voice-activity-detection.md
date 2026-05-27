# Windows Voice Activity Detection Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add default-on, conservative voice activity detection to the Windows dictation pipeline.

**Architecture:** Put the VAD decision point in `DictationController` behind `IVoiceActivityDetector`. Implement a native WAV PCM16 detector for the current NAudio recording output, and fail open on unsupported files.

**Tech Stack:** .NET 10, xUnit, WinUI 3, NAudio-produced PCM16 WAV files.

---

### Task 1: Core VAD Gate

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/VoiceActivityResult.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IVoiceActivityDetector.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Dictation/DictationController.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Dictation/DictationControllerTests.cs`

- [x] **Step 1: Add failing dictation tests**

Test that enabled/no-speech skips transcription and that disabled VAD bypasses analysis.

- [x] **Step 2: Run focused tests red**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~DictationControllerTests.StopAsync_WhenVad"
```

- [x] **Step 3: Add contracts and controller gate**

Add `VoiceActivityResult`, `IVoiceActivityDetector`, `AppSettings.IsVadEnabled`, and the no-speech early return.

- [x] **Step 4: Run focused tests green**

Expected: VAD dictation tests pass.

### Task 2: Native Detector And Settings UI

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/WavVoiceActivityDetector.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Audio/WavVoiceActivityDetectorTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [x] **Step 1: Add failing native tests**

Test silent PCM16 WAV, voiced PCM16 WAV, and fail-open missing-file behavior.

- [x] **Step 2: Implement detector**

Parse PCM16 WAV data chunks and require at least 150 ms above threshold to report speech.

- [x] **Step 3: Wire settings and UI**

Add the default-on setting, WinUI checkbox, immediate save handler, and pass the detector to `DictationController`.

- [x] **Step 4: Run focused tests and app build**

Expected: Core VAD tests, detector/settings tests, and app project build pass.

### Task 3: Docs And Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Update tracker**

Record VAD in the core dictation and settings parity notes.

- [x] **Step 2: Run full verification**

Run full solution tests, Debug x64 build, and `git diff --check`.

- [x] **Step 3: Commit**

Commit with:

```powershell
git add docs/superpowers VoiceInk.Windows
git commit -m "feat(windows): add conservative voice activity detection"
```
