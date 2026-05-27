# Windows Onboarding Desktop Microphone Guidance

## Goal

Align first-run microphone troubleshooting copy with the exact Windows desktop-app privacy toggle so new users can find the setting that affects source-built VoiceInk.

## Source Of Truth

- Microsoft documents the desktop-app microphone privacy setting as "Let desktop apps access your microphone".
- Source-built desktop apps may not appear as individual app entries in Windows microphone privacy settings.
- Onboarding should guide users without changing global machine privacy settings automatically.

## Requirements

- The onboarding checklist, summary, and setup action copy use the exact "Let desktop apps access your microphone" wording.
- The copy still explains that source-built VoiceInk may not be listed by name.
- The onboarding flow remains completable when no microphone is visible, with a clear advisory/manual path.

## Acceptance

- Focused tests fail before implementation because onboarding still uses generic desktop-app access copy.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
