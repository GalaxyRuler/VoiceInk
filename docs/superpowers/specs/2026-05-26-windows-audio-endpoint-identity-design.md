# Windows Audio Endpoint Identity Design

## Goal

Make Windows audio input selection more faithful and stable by persisting Windows Core Audio endpoint IDs alongside the existing WaveIn device number/name identity.

## Grounding

- macOS audio input selection uses stable device identifiers and supports System Default, Custom Device, and Prioritized modes.
- Microsoft documents `IMMDevice::GetId` as returning the endpoint ID string that identifies an audio endpoint device.
- NAudio exposes capture endpoints through `MMDeviceEnumerator`, while the current Windows recorder still captures through `WaveInEvent` device numbers.

## Behavior

- Keep WaveIn device numbers for recording so the capture backend stays unchanged.
- Add endpoint ID metadata to `AudioInputDevice`, `AudioInputDeviceChoice`, and `PrioritizedAudioInputDevice`.
- Persist the selected custom microphone endpoint ID in JSON settings and settings backups.
- Select custom microphones by endpoint ID first, then fall back to the existing device number/name and unique-name rebinding behavior.
- Select prioritized microphones by endpoint ID when available, then fall back to name matching for older settings.
- If Core Audio endpoint enumeration is unavailable, keep listing WaveIn devices without endpoint IDs and preserve existing behavior.

## Verification

- Add Core tests for custom endpoint rebinding and prioritized endpoint matching.
- Update settings/backup persistence tests to include endpoint IDs.
- Run full solution tests, Debug x64 build, and `git diff --check`.
