# Windows Settings Diagnostics Recent Events

## Goal

Make the Windows Settings diagnostics guidance explicit that copied diagnostics use a bounded set of recent sanitized VoiceInk status events, not an unlimited activity history.

## Source Of Truth

- The Windows app keeps recent in-memory diagnostic events for the Copy Diagnostics Summary action.
- Microsoft documents Windows Diagnostic Data Viewer and App diagnostics as OS privacy surfaces controlled through Windows settings.
- The open-source Windows fork must avoid telemetry and must describe local diagnostic exports in plain user-facing language.

## Requirements

- Add a Settings diagnostics guidance row for recent bounded events.
- State that diagnostics summary includes recent sanitized VoiceInk status events only.
- Keep the copy presenter-backed and covered by Core tests.
- Do not change diagnostic collection, retention count, export behavior, or OS privacy settings.

## Non-Goals

- No telemetry.
- No persistent analytics database.
- No Windows Diagnostic Data Viewer integration.
