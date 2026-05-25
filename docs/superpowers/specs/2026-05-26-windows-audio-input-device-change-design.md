# Windows Audio Input Device Change Design

## Goal

Refresh the Windows audio input chooser when microphones are added, removed, disabled, enabled, or when the default capture device changes.

## External Grounding

Microsoft's Core Audio documentation describes endpoint notifications through `IMMNotificationClient`, including default-device and device-state changes. NAudio exposes `MMDeviceEnumerator.RegisterEndpointNotificationCallback`, so the Windows fork can subscribe without adding a new dependency.

## Behavior

- Add a native audio input device change watcher.
- Watcher listens to Core Audio endpoint notifications through NAudio.
- Watcher debounces repeated notifications before raising `DevicesChanged`.
- Default-device notifications refresh only for capture/all data flows.
- The main window refreshes the existing audio input chooser when notified.
- If the selected custom device is rebound or missing, existing `AudioInputDeviceSelection` warning behavior remains the source of truth.
- If recording or another operation is active, the app avoids rebuilding the controller mid-operation and reports that audio inputs changed.

## Non-Goals

- No new audio backend.
- No microphone permission UI in this slice.
- No custom per-device health diagnostics in this slice.
