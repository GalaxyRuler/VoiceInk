# Windows Dictionary Workflow Directory Spec

## Intent

Make the Windows Dictionary page easier to scan by surfacing the expected workflow before the detailed vocabulary and replacement editors.

## Requirements

- Add a presenter-backed workflow directory for Add, Replace, Review, and Backup paths.
- Keep all dictionary data behavior unchanged: vocabulary, replacements, sorting, import/export, quick add, and transcript cleanup must continue to use existing services.
- Expose accessible names for each repeated workflow row with title, value, status, and detail.
- Keep copy in Core so the workflow can be tested without UI automation.

## Open-Source Boundary

The directory is local UI guidance only. It adds no accounts, telemetry, licensing, or paid feature surface.

## Verification

- Focused Core presenter tests.
- Focused XAML accessibility tests.
- WinUI app build because the Dictionary page XAML changed.
