# Windows Settings Manual Backup Profile Row Design

## Context

VoiceInk for Windows already exposes backup guidance for settings, prompts, Power Mode, model references, dictionary data, and excluded API keys. The Settings page did not separately state that backups are user-chosen export files from the current Windows profile and are not automatic roaming/sync.

Microsoft's Windows app-data guidance distinguishes local app data from roaming or backup behavior. The Windows fork should make that boundary explicit in the Settings presenter so users understand the open-source, local-first behavior.

## Goal

Add a Settings backup guidance row:

- title `Windows Profile`;
- value `User-chosen file`;
- detail `Backups are written only when you choose an export location; VoiceInk does not roam settings automatically.`;
- status badge `Manual`.

## Non-Goals

- No backup file format change.
- No automatic sync or cloud backup.
- No credential export.
- No settings import behavior change.

## Testability

Focused Settings presenter tests cover the new backup guidance row.
