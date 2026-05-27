# Windows Audio System Default Guidance Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Show Windows Sound Settings and Microphone Privacy guidance when users intentionally record with System Default audio input.

**Architecture:** Keep the behavior in `AudioInputDeviceHealthPresenter`, which already owns testable Device Health rows. Do not change capture selection or platform services.

**Tech Stack:** .NET 10, xUnit, WinUI-bound Core presenter rows.

---

### Task 1: Selected System Default Guidance Rows

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Audio/AudioInputDeviceHealthPresenterTests.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDeviceHealthPresenter.cs`
- Modify: `docs/superpowers/project-completion.md`

- [x] **Step 1: Write the failing test**

Added a test that calls `AudioInputDeviceHealthPresenter.BuildRows` with `selectedChoice` set to the `System Default` row and asserts that the rows include:

```text
Windows Sound Settings, Open Settings, Open ms-settings:sound to choose or test the Windows default input device.
Microphone Privacy, Check Access, Open ms-settings:privacy-microphone and enable 'Let desktop apps access your microphone' if Windows blocks recording.
```

- [x] **Step 2: Run RED**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioInputDeviceHealthPresenterTests.BuildRows_SelectedSystemDefault_ShowsWindowsGuidanceRows -nr:false -p:UseSharedCompilation=false
```

Result: FAIL because the selected System Default health list did not contain the guidance rows.

- [x] **Step 3: Implement minimal presenter change**

Updated `BuildChoiceRows` so it appends the existing Sound Settings and Microphone Privacy informational rows only when the selected choice is `System Default`.

- [x] **Step 4: Run GREEN and broader verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter AudioInputDeviceHealthPresenterTests -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter 'FullyQualifiedName~AudioInputDevice' -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
```

Result: Focused presenter tests passed 3/3, focused audio-device tests passed 22/22, full solution tests passed Core 823/823 and Infrastructure 267/267, Debug x64 build passed with 0 warnings and 0 errors.

- [x] **Step 5: Update tracker and commit**

Set Audio input to 99%, updated Current Slice, ran `git diff --check`, then committed:

```powershell
git add VoiceInk.Windows\src\VoiceInk.Windows.Core\Audio\AudioInputDeviceHealthPresenter.cs VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\Audio\AudioInputDeviceHealthPresenterTests.cs docs\superpowers\specs\2026-05-27-windows-audio-system-default-guidance.md docs\superpowers\plans\2026-05-27-windows-audio-system-default-guidance.md docs\superpowers\project-completion.md
git commit -m "feat(windows): add system default audio guidance"
```
