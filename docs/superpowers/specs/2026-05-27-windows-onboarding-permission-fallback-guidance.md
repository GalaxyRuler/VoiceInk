# Windows Onboarding Permission Fallback Guidance Spec

Date: 2026-05-27

## Context

VoiceInk for Windows first-run onboarding already covers local model setup, microphone detection, shortcut configuration, tutorial steps, readiness summary rows, and a Windows microphone privacy advisory. The remaining parity gap for this slice is the fallback path when the Windows microphone privacy link does not open or the user needs to navigate manually.

Microsoft's Windows microphone privacy guidance states that users control microphone access in Windows Settings, including desktop app access. VoiceInk should make the manual path explicit during onboarding without attempting to change system privacy settings automatically.

## Requirements

- Onboarding setup actions must include a `Manual Privacy Path` row.
- The row must point to `Settings > Privacy & security > Microphone` in plain language.
- The row must appear beside microphone setup actions and remain advisory.
- The row must be core presenter output with unit-test coverage.

## Non-Goals

- Do not alter Windows privacy settings automatically.
- Do not add registry or policy probes.
- Do not change onboarding completion rules.
- Do not add a new UI route.

## Acceptance Criteria

- `OnboardingChecklistPresenter.Present` returns `Manual Privacy Path` after `Check Microphone`.
- Focused onboarding presenter tests cover title, description, command text, and status badge.
- Project completion documentation reflects the onboarding permission fallback slice.
- Focused tests, full solution tests, Debug x64 build, and whitespace validation pass.
