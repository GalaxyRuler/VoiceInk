# Windows Dictionary Import Conflict Guidance Spec

Date: 2026-05-27

## Context

VoiceInk for Windows already supports dictionary vocabulary, replacements, quick add, import/export, local backup guidance, disabled replacement guidance, and processing-order guidance. The remaining parity gap for this slice is import conflict visibility: users should know imports are local JSON merges that skip duplicates rather than silently overwriting existing dictionary entries.

Windows app-data guidance recommends keeping valuable user data local and explicit. Dictionary entries are user-authored terminology and replacements, so conflict behavior should be visible before bulk import/edit flows.

## Requirements

- The Dictionary page presenter must include an `Import Conflicts` rule guidance row.
- The row must state that duplicate vocabulary words and replacement keys are skipped so existing local entries stay in place.
- The row must sit near Import / Export and Backup Workflow guidance.
- The row must be covered by presenter unit tests.

## Non-Goals

- Do not change import merge behavior in this slice.
- Do not add an import preview UI.
- Do not add cloud sync.
- Do not alter dictionary JSON schema.

## Acceptance Criteria

- `DictionaryPagePresenter.Present` returns an `Import Conflicts` row after `Import / Export`.
- Focused Dictionary presenter tests cover the row title, value, detail, and status badge.
- Project completion documentation reflects the dictionary import-conflict slice.
- Focused tests, full solution tests, Debug x64 build, and whitespace validation pass.
