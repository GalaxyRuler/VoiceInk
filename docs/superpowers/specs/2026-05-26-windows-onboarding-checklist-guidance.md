# Windows Onboarding Checklist Guidance Spec

## Goal

Improve the Windows first-run setup dialog so it communicates VoiceInk's setup journey with macOS-aligned steps: welcome, microphone/device readiness, shortcut setup, local model selection, and a first dictation smoke path.

## Source Of Truth

- `VoiceInk/Views/Onboarding/OnboardingView.swift`
- `VoiceInk/Views/Onboarding/OnboardingPermissionsView.swift`
- `VoiceInk/Views/Onboarding/OnboardingModelDownloadView.swift`
- `VoiceInk/Views/Onboarding/OnboardingTutorialView.swift`
- Public VoiceInk docs describe setup around microphone input, transcription setup, shortcuts, privacy, and Power Mode.

## Requirements

- Keep the Windows implementation open-source only and local-first by default.
- Keep setup logic in `VoiceInk.Windows.Core` and UI rendering in the WinUI app.
- Show a progress summary that tells the user how many setup essentials are ready.
- Show an action-oriented checklist with model, shortcut, microphone, privacy, and try-it-out guidance.
- Keep microphone availability as advisory because a privacy block can hide devices until runtime.
- Keep setup completable when model path and primary shortcut are configured, matching the existing Windows MVP behavior.
- Do not add secrets, accounts, licensing, trials, upgrade prompts, or commercial telemetry.

## Non-Goals

- Do not redesign the onboarding dialog into a multi-page animated flow in this slice.
- Do not add new provider credentials or model download sources in this slice.
- Do not require UI automation for verification.

## Acceptance Criteria

- Core tests cover incomplete, complete, and no-microphone onboarding presentation states.
- The first-run dialog displays a concise progress label and a checklist instead of only raw health lines.
- The existing detailed health summary remains available in the dialog.
- Full solution tests and Debug x64 build pass.
