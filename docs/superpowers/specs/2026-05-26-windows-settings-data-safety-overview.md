# Windows Settings Data Safety Overview Spec

## Goal

Add a concise Settings overview that explains the Windows fork's local-only backup, privacy cleanup, and diagnostics behavior.

## Source Of Truth

- `VoiceInk/Views/Settings/*`
- `VoiceInk/Views/Privacy/*`
- VoiceInk public docs for general settings, audio input, and troubleshooting.

## Requirements

- Keep overview copy in `VoiceInk.Windows.Core`.
- State that settings backups are local JSON and exclude API keys.
- State that diagnostics are local and sanitized, with no telemetry.
- State that privacy cleanup affects only local transcripts/audio according to user retention settings.
- Do not add updater, account, licensing, paid telemetry, or commercial language.

## Acceptance Criteria

- Core tests cover Settings overview copy and the existing section list.
- The Settings page displays the overview under the hero description.
- Full solution tests and Debug x64 build pass.
