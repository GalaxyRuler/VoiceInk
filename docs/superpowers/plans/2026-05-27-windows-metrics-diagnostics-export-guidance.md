# Windows Metrics Diagnostics Export Guidance Plan

Date: 2026-05-27

## Goal

Clarify in Metrics diagnostics that VoiceInk CSV export is local app metrics and separate from Windows Diagnostic Data Viewer exports.

## Steps

1. Add a failing Metrics dashboard presenter assertion for a `Windows Diagnostics` diagnostics row.
2. Add the diagnostics row to `SessionMetricsDashboardPresenter`.
3. Run the focused Metrics dashboard presenter tests.
4. Update the project completion tracker and slice documentation.
5. Run the full verification gate:
   - `dotnet test VoiceInk.Windows\VoiceInk.Windows.sln`
   - `dotnet build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64`
   - `git diff --check`
6. Commit the completed slice.

## Review Notes

- Keep VoiceInk metrics local and user-initiated.
- Do not imply access to Windows diagnostic events.
- Preserve reset semantics: metrics only, History and recordings untouched.
