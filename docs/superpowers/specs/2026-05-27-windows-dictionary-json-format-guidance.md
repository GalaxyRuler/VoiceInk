# Windows Dictionary JSON Format Guidance

## Goal

Make the Dictionary import/export format explicit so Windows users understand that VoiceInk uses local JSON, not spreadsheet CSV, for Unicode-safe dictionary backups.

## Source Of Truth

- Microsoft documents CSV encoding/BOM handling as a common Excel-on-Windows concern for UTF-8 text.
- VoiceInk Windows dictionary import/export is local VoiceInk JSON, not CSV.
- Dictionary entries may contain product names, URLs, non-English vocabulary, and punctuation that should round-trip without spreadsheet encoding assumptions.

## Requirements

- Dictionary local-backup guidance says import/export uses local VoiceInk JSON and avoids CSV encoding issues.
- Dictionary rule guidance includes a `File Format` row: `JSON, not CSV`.
- The guidance does not claim CSV support and does not add sync, cloud import, or account-based backup.

## Acceptance

- Focused tests fail before implementation because the JSON/Unicode-safe guidance is absent.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
