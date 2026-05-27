# Windows Metrics Diagnostic Viewer Boundary

## Goal

Clarify that VoiceInk metrics exports are local app metrics and are separate from Windows Diagnostic Data Viewer history, storage, and export behavior.

## Source Of Truth

- Microsoft documents Diagnostic Data Viewer as a Windows-owned viewer for diagnostic data sent by the device.
- Microsoft also documents that enabling data viewing can store diagnostic data locally while Windows controls that history and storage.
- VoiceInk Windows metrics are local SQLite productivity/session summaries and must not imply Windows diagnostic telemetry is enabled, exported, or controlled by VoiceInk.

## Requirements

- Metrics diagnostics rows explain that VoiceInk metrics export is separate from Windows Diagnostic Data Viewer.
- The row also states that Windows controls its own diagnostic data history and storage.
- No telemetry, Windows diagnostic setting mutation, or Diagnostic Data Viewer integration is added.

## Acceptance

- Focused tests fail before implementation because the row only mentions separate exports.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
