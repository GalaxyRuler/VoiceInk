# Windows Onboarding Health Checklist Design

## Goal

Make first-run setup more faithful to VoiceInk's guided setup by showing a broader readiness checklist instead of a single microphone sentence.

## Grounding

Windows microphone access depends on the app manifest capability, OS privacy settings, and an available capture device. The Windows fork already declares the microphone capability and opens `ms-settings:privacy-microphone` for remediation. This slice exposes those readiness checks in onboarding without changing global machine settings.

## Requirements

- First-run setup shows a checklist covering:
  - model path;
  - primary shortcut;
  - microphone device detection;
  - Windows microphone privacy/settings;
  - app microphone capability.
- Checklist state updates when model path, shortcut, or microphone refresh state changes.
- The existing save requirements remain model path + shortcut; missing microphone remains a warning so users can finish setup on machines where hardware is attached later.
- No global permission or privacy settings are changed automatically.

## Non-Goals

- No automatic microphone permission grants.
- No machine-wide settings changes.
- No audio capture during onboarding.
- No fragile UI automation for Windows Settings.

## Verification

- Focused core tests cover checklist text for ready and attention states.
- Debug x64 build validates the first-run dialog wiring.
