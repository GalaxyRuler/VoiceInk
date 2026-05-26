# Windows Audio Input Priority Design

## Goal

Mirror the macOS Audio Input page's `System Default`, `Custom Device`, and `Prioritized` modes in the Windows fork. Prioritized mode should try microphones in user-defined order, skip unavailable entries, and fall back gracefully to the Windows system default when no prioritized microphone is present.

## Grounding

- macOS source of truth: `VoiceInk/Services/AudioDeviceManager.swift` and `VoiceInk/Views/Settings/AudioInputSettingsView.swift`.
- Windows source: the current MVP uses NAudio `WaveIn` device numbers for capture and already has saved-name rebinding plus Core Audio device-change refresh.
- Microsoft Core Audio docs describe capture endpoint enumeration and endpoint notifications through MMDevice/IMMDeviceEnumerator. The richer endpoint ID path remains a future backend improvement because the existing Windows capture pipeline is WaveIn-number based.

## Behavior

- `System Default` mode follows the Windows default microphone and stores no custom device number/name.
- `Custom Device` mode preserves the existing single selected microphone behavior, including saved-name rebinding when the WaveIn device number changes.
- `Prioritized` mode stores a normalized ordered list of microphone names. At selection time, VoiceInk chooses the first available priority entry by exact name.
- If the first priority entry is unavailable but a later entry is available, the Audio Input status shows a warning fallback notice and uses the later microphone.
- If no priority entry is available, VoiceInk selects `System Default` and shows a warning notice.
- Existing settings files with only `AudioInputDeviceNumber`/`AudioInputDeviceName` are treated as legacy custom-device selections when loaded into the UI.

## Scope

- Add Core settings for audio input mode and priority entries.
- Add Core selection/list tests.
- Add WinUI controls for mode, priority list, add/remove, and move up/down.
- Persist the mode and list in JSON settings and settings backup files.
- Do not replace NAudio capture with WASAPI/MMDevice in this slice.

## Verification

- Focused Core tests for audio input selection and priority list behavior.
- Focused Infrastructure settings persistence test.
- Full solution build because XAML and event hookups are touched.
