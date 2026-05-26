# Windows Settings Action Summary Spec

Date: 2026-05-26

## Goal

Make the long Windows Settings page easier to scan by adding a presenter-backed action summary for shortcuts, safety, backups, and diagnostics.

## Windows Behavior

- Extend `SettingsSectionPresenter` with summary rows.
- Show four rows: Shortcuts, Data Safety, Backup, Diagnostics.
- Render the rows under the Settings hero before the detailed settings controls.
- Do not change settings persistence, shortcut registration, cleanup, backup, diagnostics, or recording feedback behavior.
- Keep all copy open-source and local-only.

