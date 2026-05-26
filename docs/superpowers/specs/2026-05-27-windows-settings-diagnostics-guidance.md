# Windows Settings Diagnostics Guidance Design

## Context

VoiceInk for Windows already exports sanitized diagnostics, opens the diagnostics folder, and presents local backup/privacy guidance. The diagnostics area still needed the same scan-friendly, local-first disclosure used in other Windows parity slices.

Windows also has OS-level app diagnostics privacy controls. VoiceInk should be explicit that its own diagnostics exports are local and sanitized, while any broader app-diagnostics capability is governed by Windows privacy settings.

## Goal

Add presenter-backed diagnostics guidance rows:

- diagnostic logs are local exports and are not sent automatically;
- copied summaries are sanitized and exclude credentials;
- Windows app diagnostics are OS-controlled outside VoiceInk's local logs.

## Non-Goals

- No telemetry.
- No automatic upload.
- No change to log collection scope.
- No settings schema change.

## Testability

`SettingsSectionPresenter` exposes diagnostics guidance rows covered by Core tests, and the existing About/Diagnostics surface renders those rows with WinUI binding.
