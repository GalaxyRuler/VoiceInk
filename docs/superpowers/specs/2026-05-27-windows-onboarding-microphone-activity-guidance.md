# Windows Onboarding Microphone Activity Guidance

## Goal

Make first-run microphone troubleshooting clearer for source-built Windows users by mentioning Windows' recent desktop-app microphone activity surface.

## Source Of Truth

- Microsoft documents that Windows microphone privacy settings include desktop-app access controls that may differ from Store app per-app listings.
- Microsoft privacy guidance also describes recent resource-access activity for microphone and other device capabilities.
- VoiceInk for Windows already guides users to enable Microphone access and `Let desktop apps access your microphone`; this slice adds where to look when the app is not listed by name.

## Requirements

- Extend the onboarding manual privacy path row with recent desktop-app microphone activity guidance.
- Keep the guidance in Core presenter text so it remains testable and UI-independent.
- Do not add registry edits, global privacy changes, telemetry, or destructive diagnostics.
