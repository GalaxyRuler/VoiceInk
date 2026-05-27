# Windows Settings Grouping Guidance Spec

Date: 2026-05-27

## Context

VoiceInk for Windows already exposes Settings sections for shortcuts, recording feedback, interface, clipboard, cleanup, privacy, general options, backup, and diagnostics. The remaining parity gap for this slice is discoverability: the settings overview should tell users that the long page is intentionally grouped by purpose.

Microsoft Windows app settings guidance recommends making settings easy to find and grouping related app-customizable options. VoiceInk should follow that guidance while preserving macOS VoiceInk terminology and the existing open-source local data-safety posture.

## Requirements

- The Settings section presenter must include a `Find Settings` action summary.
- The row must explain that settings are grouped across shortcuts, feedback, interface, clipboard, cleanup, privacy, backup, and diagnostics.
- The row must be core presenter data with unit-test coverage.
- Do not add account, licensing, upsell, telemetry, or commercial settings surfaces.

## Non-Goals

- Do not add a search control in this slice.
- Do not reorder the actual Settings page sections.
- Do not change settings persistence.
- Do not add a dependency.

## Acceptance Criteria

- `SettingsSectionPresenter.Present` returns a `Find Settings` action summary after `Shortcuts`.
- The focused Settings presenter test covers the row title, description, and status badge.
- Project completion documentation reflects the Settings grouping slice.
- Focused tests, full solution tests, Debug x64 build, and whitespace validation pass.
