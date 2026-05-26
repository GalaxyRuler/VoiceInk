# Windows Dictionary Backup Workflow Guidance Design

## Context

VoiceInk for Windows supports vocabulary words, replacements, quick add, and local JSON import/export. The dictionary page already explains that import/export is local and does not sync automatically, but it did not separately explain the user workflow for exporting before bulk edits or restoring entries later.

Dictionary and vocabulary tools commonly present export as a local backup/edit/revert workflow. The Windows fork should expose the same practical guidance without adding CSV support, cloud sync, accounts, or commercial surfaces.

## Goal

Add a rule guidance row to `DictionaryPagePresenter`:

- title `Backup Workflow`;
- value `Edit or restore`;
- detail `Export before bulk edits so you can review, edit, or restore dictionary entries later.`;
- status badge `Backup`.

## Non-Goals

- No new file format. The Windows fork continues to use local VoiceInk dictionary JSON.
- No automatic sync or cloud backup.
- No import behavior changes.
- No telemetry or commercial gating.

## Testability

Focused dictionary presenter tests cover the new backup workflow guidance row.
