# Windows Onboarding Permission Summary Design

## Context

VoiceInk onboarding guides users through model selection, microphone selection, shortcuts, and a first dictation smoke path. Windows microphone access can still be blocked by OS privacy settings, and desktop apps may not appear in every app-permission list. Users need that boundary visible during first-run setup, not only on the later Permissions page.

## Goal

Add a Windows Permission summary row to onboarding:

- when microphones are visible, remind users to keep Windows microphone and desktop app access enabled;
- when no microphone is visible, direct users to Windows microphone privacy settings after refreshing devices;
- keep onboarding completable so users can continue setup and resolve hardware/privacy issues afterward.

## Non-Goals

- No new OS permission API.
- No blocking onboarding on microphone availability.
- No changes to recording or audio device selection.

## Testability

`OnboardingChecklistPresenter` produces the permission summary row and focused Core tests cover ready and missing-microphone states. The existing onboarding dialog renders summary rows.
