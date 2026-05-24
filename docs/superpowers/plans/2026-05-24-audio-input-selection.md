# Audio Input Selection Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let Windows users refresh available microphones and choose either the Windows system default or a specific capture device before recording.

**Architecture:** Preserve the existing `NAudioCaptureService` as the recorder boundary and add a small Core device model plus Native NAudio enumerator. Store the selected device number and display name in JSON settings, then let the WinUI shell recreate the dictation controller whenever recording starts with a different selected device.

**Tech Stack:** .NET 10, WinUI 3, NAudio `WaveIn.DeviceCount`, `WaveIn.GetCapabilities`, `WaveInEvent.DeviceNumber`, existing JSON settings store.

---

## Source Notes

- macOS source of truth: `VoiceInk/Services/AudioDeviceManager.swift` supports System Default, Custom Device, and Prioritized modes.
- macOS UI reference: `VoiceInk/Views/Settings/AudioInputSettingsView.swift` exposes refresh, active/unavailable device states, and input mode choices.
- Online grounding: NAudio's repository documents recording with WaveIn/WASAPI/ASIO and device enumeration support; Mark Heath's NAudio guidance uses `WaveIn.DeviceCount` plus `WaveIn.GetCapabilities(index)` to enumerate recording devices, and `WaveInEvent.DeviceNumber` selects the recording device.
- Windows adaptation: this slice implements System Default and Custom Device. Prioritized mode, automatic device-change notifications, and active/unavailable badges remain follow-up work.

## File Structure

- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDevice.cs`: immutable capture device identity for Core/App use.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IAudioInputDeviceProvider.cs`: UI-independent enumeration contract.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`: add selected audio input device fields.
- Modify `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`: verify settings round-trip.
- Create `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioInputDeviceProvider.cs`: enumerate active WaveIn capture devices.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioCaptureService.cs`: accept an optional device number and apply it to `WaveInEvent`.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`: add Audio Input controls.
- Modify `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`: load, refresh, save, and apply the selected capture device.
- Modify `README.md`: document audio input selection in the Windows MVP.
- Modify `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`: move refresh/custom device support from gap to implemented and leave prioritized mode as a gap.
- Update this plan with verification status.

## Task 1: Settings Contract

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Settings/AppSettings.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Settings/JsonSettingsStoreTests.cs`

- [x] **Step 1: Write failing settings persistence test**

Extend `SaveAsync_PersistsSettings` so the expected settings include:

```csharp
AudioInputDeviceNumber = 2,
AudioInputDeviceName = "USB Microphone"
```

The existing `Assert.Equal(expected, actual)` should prove the new fields round-trip through JSON.

- [x] **Step 2: Verify RED**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter SaveAsync_PersistsSettings
```

Expected before implementation: build fails because `AudioInputDeviceNumber` and `AudioInputDeviceName` do not exist.

- [x] **Step 3: Add settings fields**

Add to `AppSettings`:

```csharp
public int? AudioInputDeviceNumber { get; init; }
public string AudioInputDeviceName { get; init; } = string.Empty;
```

- [x] **Step 4: Verify GREEN**

Run the same filtered settings test.

Expected: `SaveAsync_PersistsSettings` passes.

- [ ] **Step 5: Commit**

Commit message:

```text
feat(windows): add audio input settings
```

## Task 2: Native Device Enumeration And Capture Selection

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Audio/AudioInputDevice.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Services/IAudioInputDeviceProvider.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioInputDeviceProvider.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Audio/NAudioCaptureService.cs`

- [ ] **Step 1: Add Core model and provider contract**

Create:

```csharp
namespace VoiceInk.Windows.Core.Audio;

public sealed record AudioInputDevice(int DeviceNumber, string Name, int Channels);
```

Create:

```csharp
using VoiceInk.Windows.Core.Audio;

namespace VoiceInk.Windows.Core.Services;

public interface IAudioInputDeviceProvider
{
    Task<IReadOnlyList<AudioInputDevice>> ListInputDevicesAsync(CancellationToken cancellationToken);
}
```

- [ ] **Step 2: Implement NAudio enumeration**

Create `NAudioInputDeviceProvider`:

```csharp
using NAudio.Wave;
using VoiceInk.Windows.Core.Audio;
using VoiceInk.Windows.Core.Services;

namespace VoiceInk.Windows.Native.Audio;

public sealed class NAudioInputDeviceProvider : IAudioInputDeviceProvider
{
    public Task<IReadOnlyList<AudioInputDevice>> ListInputDevicesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var devices = new List<AudioInputDevice>();
        for (var deviceNumber = 0; deviceNumber < WaveIn.DeviceCount; deviceNumber++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var capabilities = WaveIn.GetCapabilities(deviceNumber);
            devices.Add(new AudioInputDevice(deviceNumber, capabilities.ProductName, capabilities.Channels));
        }

        return Task.FromResult<IReadOnlyList<AudioInputDevice>>(devices);
    }
}
```

- [ ] **Step 3: Add selected device support to capture service**

Change the constructor to:

```csharp
public sealed class NAudioCaptureService(string recordingsDirectory, int? deviceNumber = null) : IAudioCaptureService, IDisposable
```

Then, after creating `WaveInEvent`, apply the selected device only when present:

```csharp
if (deviceNumber is not null)
{
    waveIn.DeviceNumber = deviceNumber.Value;
}
```

Keep the default path unchanged when `deviceNumber` is null.

- [ ] **Step 4: Build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [ ] **Step 5: Commit**

Commit message:

```text
feat(windows): enumerate audio input devices
```

## Task 3: Shell Audio Input Controls

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.App/MainWindow.xaml.cs`

- [ ] **Step 1: Add XAML controls**

Insert an `Audio Input` section after the model path field with:

```xml
<StackPanel Grid.Row="2" Spacing="8">
    <TextBlock Text="Audio Input" FontWeight="SemiBold" />
    <ComboBox
        x:Name="AudioInputComboBox"
        Header="Microphone"
        PlaceholderText="System Default" />
    <StackPanel Orientation="Horizontal" Spacing="8">
        <Button x:Name="RefreshAudioInputsButton" Content="Refresh" Click="RefreshAudioInputsButton_Click" />
        <Button x:Name="ApplyAudioInputButton" Content="Apply Audio Input" Click="ApplyAudioInputButton_Click" />
    </StackPanel>
</StackPanel>
```

Increment the following `Grid.Row` values by one and add one `RowDefinition`.

- [ ] **Step 2: Add shell device choice model**

Add:

```csharp
private sealed record AudioInputDeviceChoice(int? DeviceNumber, string Name)
{
    public override string ToString() => DeviceNumber is null ? "System Default" : $"{Name} ({DeviceNumber})";
}
```

Add fields:

```csharp
private readonly NAudioInputDeviceProvider audioInputDeviceProvider;
private IReadOnlyList<AudioInputDeviceChoice> audioInputChoices = [];
private int? activeAudioInputDeviceNumber;
```

- [ ] **Step 3: Load and refresh device list**

During construction initialize `audioInputDeviceProvider = new NAudioInputDeviceProvider();`.

During `InitializeAsync`, after loading settings, call:

```csharp
await RefreshAudioInputDevicesAsync(settings, windowLifetime.Token);
```

Implement `RefreshAudioInputDevicesAsync` so it:

- Starts with a `System Default` choice.
- Appends devices from `audioInputDeviceProvider.ListInputDevicesAsync`.
- Selects the saved device number when it is still present.
- Falls back to System Default and reports `Selected audio input is unavailable; using System Default` when the saved device is missing.

- [ ] **Step 4: Save and apply selected input**

Update `CurrentSettingsAsync` to persist:

```csharp
AudioInputDeviceNumber = SelectedAudioInputDeviceNumber(),
AudioInputDeviceName = SelectedAudioInputDeviceName()
```

Implement `ApplyAudioInputAsync` so it saves settings, recreates the controller if not recording, and reports `Audio input updated`.

Change `RecreateController()` to pass `SelectedAudioInputDeviceNumber()` into `NAudioCaptureService`.

In `StartCurrentRecordingAsync`, call `EnsureControllerMatchesSelectedAudioInput()` before starting.

- [ ] **Step 5: Build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [ ] **Step 6: Commit**

Commit message:

```text
feat(windows): expose audio input selection
```

## Task 4: Docs, Review, And Verification

**Files:**

- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/plans/2026-05-24-audio-input-selection.md`

- [ ] **Step 1: Update docs**

Document:

- Windows shell can refresh input devices.
- Windows shell can use System Default or a selected device.
- Prioritized mode and live device-change fallback remain gaps.

- [ ] **Step 2: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

Expected: all tests pass.

- [ ] **Step 3: Run Debug x64 build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

Expected: build succeeds with 0 errors.

- [ ] **Step 4: Request review and fix Important findings**

Review the slice from this plan commit through HEAD. Fix Critical and Important findings before proceeding.

- [ ] **Step 5: Commit docs**

Commit message:

```text
docs(windows): note audio input selection
```

## Plan Self-Review

Spec coverage:

- Covers Windows audio device refresh, System Default mode, custom device selection, settings persistence, and capture service wiring.
- Leaves prioritized ordering and live device-change notifications as explicit follow-up gaps.

Placeholder scan:

- No placeholder sections remain.

Type consistency:

- Uses `AudioInputDevice`, `IAudioInputDeviceProvider`, `NAudioInputDeviceProvider`, `AudioInputDeviceNumber`, and `AudioInputDeviceName` consistently.
