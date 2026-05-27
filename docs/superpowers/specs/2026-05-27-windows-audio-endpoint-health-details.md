# Windows Audio Endpoint Health Details Spec

Date: 2026-05-27

## Goal

Make Windows Audio Input diagnostics more endpoint-native by showing the Windows endpoint identity in Device Health rows when a physical microphone exposes one.

## Source Of Truth

Microsoft Core Audio documents `IMMDevice` as the interface for audio endpoint devices. VoiceInk already stores endpoint IDs for custom and prioritized microphone matching, so the Device Health view should disclose that identity in a readable way when it helps users distinguish renamed or same-named microphones.

## Windows Behavior

- Physical microphone Device Health rows append `Endpoint {id}` when an endpoint ID is available.
- Prioritized-mode available microphone rows also append the endpoint identity.
- System Default and unavailable saved-priority rows remain unchanged because they do not represent a current physical endpoint row.
- Long endpoint IDs are shortened in the presenter so the UI stays readable while retaining a recognizable prefix and suffix.
- Matching, recording, persistence, and fallback behavior remain unchanged.

## Non-Goals

- Do not switch capture from WaveIn device numbers to WASAPI.
- Do not expose full long endpoint identifiers in every compact row.
- Do not add probing, telemetry, driver changes, account flows, or commercial support surfaces.
