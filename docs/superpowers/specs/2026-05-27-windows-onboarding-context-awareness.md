# Windows Onboarding Context Awareness

## Goal

Bring Windows onboarding closer to the macOS permissions flow by explaining optional context awareness before first use, without pretending Windows has the same screen-recording permission model as macOS.

## Source Of Truth

- `VoiceInk/Views/Onboarding/OnboardingPermissionsView.swift` includes a Screen Recording step for contextual awareness.
- macOS copy says VoiceInk captures on-screen text to understand voice-input context and that the data is processed locally and not stored.
- Microsoft documentation for `Windows.Graphics.Capture` describes a system picker flow where the user explicitly consents to a captured window or display.

## Requirements

- Add a Windows onboarding advisory for context awareness / screen OCR.
- State that context awareness is optional, local, and default-off.
- State that Windows asks for capture consent when screen OCR context is enabled and used.
- Surface the guidance in the testable Core onboarding presenter so the UI can display it without fragile UI automation.
- Do not count optional context awareness as a required setup blocker.

## Non-Goals

- No OS permission mutation.
- No change to default context settings.
- No new capture implementation.
- No commercial or hosted documentation links.
