# Windows Dictionary Usage Summary Spec

Date: 2026-05-26

## Goal

Make the Dictionary page easier to scan by adding local usage summary rows for vocabulary, replacements, disabled replacements, and JSON import/export.

## Windows Behavior

- Extend the Core dictionary presenter with summary rows.
- Show rows under the Dictionary hero before detailed vocabulary and replacement controls.
- Rows summarize vocabulary count, active replacement count, disabled replacement count, and local JSON backup/import.
- Keep dictionary persistence, replacement cleanup, quick add, import, and export behavior unchanged.

## Open-Source Boundary

The summary is derived from local dictionary data only. No account sync, telemetry, commercial dictionary packs, paid accuracy gates, or cloud dictionary uploads are added.
