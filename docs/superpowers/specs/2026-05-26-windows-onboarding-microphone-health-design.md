# Windows Onboarding Microphone Health Design

## Goal

Make first-run onboarding clearer when microphone access or device availability is not ready, without changing Windows privacy settings automatically.

## Source Grounding

Microsoft documents `ms-settings:privacy-microphone` as the Windows Settings URI for microphone privacy, and recommends offering a convenient link when an app cannot access a sensitive resource. This slice keeps the action user-initiated: VoiceInk opens Settings only when the user clicks the button.

## Behavior

- The onboarding status model includes microphone health text derived from whether any physical input devices are available.
- When a microphone is detected, onboarding shows a ready microphone status.
- When no microphone is detected, onboarding shows a recovery-oriented status and points the user to refreshing devices or checking Windows microphone privacy settings.
- The onboarding dialog includes a `Refresh Microphones` action that refreshes the device list in-place.
- The existing `Open Windows Microphone Settings` action remains user-initiated and opens `ms-settings:privacy-microphone`.
- Lack of detected microphones does not block saving onboarding if model path and shortcut are valid, preserving the current graceful-degradation behavior.

## Non-Goals

- No automatic privacy permission changes.
- No registry/group-policy checks.
- No microphone recording probe in onboarding.

## Verification

- Core onboarding status tests cover microphone-present and microphone-missing messages.
- App build verifies the dialog wiring.
- Full solution tests and Debug x64 build pass before commit.
