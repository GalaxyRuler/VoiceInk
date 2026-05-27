# Windows Dictionary Workflow Directory Plan

## Slice

Add a compact Dictionary workflow directory that guides users from vocabulary setup through replacements, review, and local backup.

## Tasks

1. Add failing presenter and XAML accessibility coverage.
2. Extend `DictionaryPagePresenter` with workflow rows and accessible names.
3. Render the workflow rows in `MainWindow.xaml` and bind them from `MainWindow.xaml.cs`.
4. Update the completion tracker and verify focused tests plus an app build.

## Review Notes

- Keep the rows informational only.
- Do not change dictionary persistence, import/export, replacement cleanup, or enhancement prompt rendering.
- Preserve the local-only/open-source boundary.
