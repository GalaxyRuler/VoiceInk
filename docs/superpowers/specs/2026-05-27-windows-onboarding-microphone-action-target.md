# Windows Onboarding Microphone Action Target Design

## Context

VoiceInk's onboarding flow guides first-run setup for models, shortcuts, microphone access, and basic usage. The Windows UI already opens the Windows microphone privacy page, but the target URI lived only in UI code while the onboarding status model exposed only button text.

Microsoft's WinUI settings-launch guidance uses `ms-settings:privacy-microphone` for microphone privacy. The Windows fork should make that target part of the core onboarding status so the flow is testable and consistent with the dedicated Permissions page.

## Goal

Expose a stable microphone action target from `OnboardingSetupStatus`:

- target is `ms-settings:privacy-microphone` whether a microphone is currently detected or not;
- existing onboarding button labels remain unchanged;
- the onboarding dialog button uses the target from core status before launching Windows Settings;
- behavior stays local-only and does not add telemetry or commercial surfaces.

## Non-Goals

- No new permission probing API.
- No registry, policy, or global settings mutation.
- No change to the dedicated Permissions page action target.

## Testability

Focused onboarding status tests assert the microphone action target for both detected and missing microphone states.
